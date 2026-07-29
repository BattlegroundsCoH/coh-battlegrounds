## 1. Architecture summary

The lobby uses `LobbyViewModel` as a local mirror over the server-backed `ILobby`.

- Buttons generally express user intent through commands: leave, chat, ready, move, lock, and start.
- Map, setting, company, and AI selections use `TwoWay` bindings whose property setters execute asynchronous server commands.
- `MultiplayerLobby` consumes the gRPC stream, mutates its local model, and publishes `LobbyEvent` objects through a channel.
- `LobbyViewModel.PollLobbyEvents` consumes that channel and updates WPF-bound properties and collections.
- Code-behind only assigns `DataContext`, which is appropriate.
- The chat textbox is correctly a local draft; typing does not itself send network traffic.

The principal architectural problem is that several properties simultaneously represent displayed state, user drafts, optimistic state, and outbound intent.

## 2. Findings

### [Critical] A server-applied company update can be sent back to the server indefinitely

**Evidence**

The downloaded-company event is applied through:

- [`LobbyViewModel.cs:456`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/ViewModels/LobbyViewModel.cs:456): `slot with { SelectedCompany = ... }`
- [`LobbySlotViewModel.cs:80`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/ViewModels/LobbyHelpers/LobbySlotViewModel.cs:80): the `SelectedCompany` setter executes `SetCompanyCommand`.
- [`MultiplayerLobby.cs:212`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/Models/Lobbies/MultiplayerLobby.cs:212): a slot update starts another company download.
- [`MultiplayerLobby.cs:288`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/Models/Lobbies/MultiplayerLobby.cs:288): download completion publishes the company event.

**Why it matters**

For a host, the flow can become:

```text
SlotUpdated
  -> download company
  -> downloaded-company event
  -> SelectedCompany setter
  -> SetCompany RPC
  -> SlotUpdated
  -> ...
```

This is exactly the feedback loop prohibited by the review guide.

**Correction**

Do not apply downloaded/server state through a property that sends commands. Give `LobbySlotViewModel` a state-only method such as `ApplyServerCompany`, and reserve a separate command for explicit user commits.

```csharp
public void ApplyServerCompany(Company company)
{
    _selectedCompany = new PickableCompany(false, false, company);
    _companyId = company.Id;
    OnPropertyChanged(nameof(SelectedCompany));
}

// Event handler:
slot.ApplyServerCompany(company); // No RPC
```

Converting `LobbySlotViewModel` from a record into an `ObservableObject` would make this cleaner and avoid dangerous `with` setters.

---

### [Critical] Merely displaying or initializing the page can overwrite server state

**Evidence**

- [`LobbySlotViewModel.cs:51`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/ViewModels/LobbyHelpers/LobbySlotViewModel.cs:51) has an `AvailableCompanies` getter with an acknowledged side effect. Reading it assigns `SelectedCompany`, which sends an RPC.
- [`LobbyViewModel.cs:296`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/ViewModels/LobbyViewModel.cs:296) asynchronously loads local companies and then unconditionally calls `_lobby.SetCompany` at line 344.
- The getter is read by the `ItemsSource` binding at [`LobbyView.xaml:144`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/Views/LobbyView.xaml:144).

**Why it matters**

WPF can evaluate a getter repeatedly for initial layout, template creation, or binding refresh. Rendering the page can therefore select a company and publish it. Separately, initialization chooses the first local company even if the snapshot already contains a server-confirmed selection.

**Correction**

- Make every collection getter pure.
- Populate available companies during initialization and notify WPF.
- Never auto-publish from page construction.
- If auto-assignment is a product requirement, request it only when the authoritative slot is empty, and preferably let the server perform or validate that transition using an expected revision.

---

### [High] Slot updates are dropped because the event payload contract is inconsistent

**Evidence**

- [`MultiplayerLobby.cs:214`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/Models/Lobbies/MultiplayerLobby.cs:214) publishes an integer team ID for `SlotUpdated`.
- [`LobbyViewModel.cs:373`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/ViewModels/LobbyViewModel.cs:373) only accepts `TeamType`.
- [`MultiplayerLobby.cs:508`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/Models/Lobbies/MultiplayerLobby.cs:508) also publishes an integer for a local faction update.
- The existing test explicitly documents the discrepancy at [`LobbyViewModelTests.cs:541`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds.Test/ViewModels/LobbyViewModelTests.cs:541).

**Why it matters**

Both `updateTeam1` and `updateTeam2` become false, so the UI ignores valid authoritative slot changes. Players can see outdated occupants, companies, locks, or AI configuration.

**Correction**

Use one strongly typed payload everywhere, for example:

```csharp
public sealed record TeamChanged(int TeamIndex, long Revision);
```

As a minimal patch, map the integer to `team.TeamType` before creating `LobbyEvent`, and change the other producers to emit the same type.

---

### [High] Bound value setters perform unawaited network operations

**Evidence**

The following `TwoWay` bindings reach async commands through ordinary setters:

- Company: [`LobbyView.xaml:145`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/Views/LobbyView.xaml:145)
- AI difficulty: [`LobbyView.xaml:227`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/Views/LobbyView.xaml:227)
- Boolean setting: [`LobbyView.xaml:287`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/Views/LobbyView.xaml:287)
- Slider: [`LobbyView.xaml:317`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/Views/LobbyView.xaml:317)
- Option selection: [`LobbyView.xaml:341`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/Views/LobbyView.xaml:341)
- Map: [`LobbyViewModel.cs:147`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/ViewModels/LobbyViewModel.cs:147)

The helper setters call `IAsyncRelayCommand.Execute`, which cannot be awaited by the binding setter.

**Why it matters**

- Exceptions and rejection handling are detached from the initiating edit.
- Server-applied or WPF-coerced values can be mistaken for user intent.
- The slider can produce many overlapping or dropped updates.
- Settings mutate the shared `LobbySetting` before server confirmation.
- There is no pending state or reliable rollback.

**Correction**

Use draft-and-commit state:

- Property setters update drafts only.
- Explicit commands send immutable desired values.
- For sliders, commit on drag completion or debounce/coalesce changes.
- Disable commands while disconnected or while a non-reentrant request is pending.
- Reconcile drafts when the authoritative server event arrives.

An explicit Apply button is the smallest fully reliable solution for ComboBox and slider edits without adding another behavior library.

---

### [High] Remote setting updates may not refresh the controls

**Evidence**

- `SelectedSettings` is an ordinary `ICollection`, not `ObservableCollection`.
- [`LobbyViewModel.cs:409`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/ViewModels/LobbyViewModel.cs:409) removes and re-adds an item without `CollectionChanged`.
- [`LobbySettingViewModel.cs:7`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/ViewModels/LobbyHelpers/LobbySettingViewModel.cs:7) does not implement `INotifyPropertyChanged`.
- The transport mutates the existing shared `LobbySetting` at [`MultiplayerLobby.cs:225`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/Models/Lobbies/MultiplayerLobby.cs:225).

**Why it matters**

A non-host can continue seeing an old checkbox, slider, or selected option even though the server state changed. Re-raising `PropertyChanged` for the same collection reference is not a reliable substitute for item or collection notifications.

**Correction**

Keep stable setting view models in an `ObservableCollection<LobbySettingViewModel>`. Add an `ApplyConfirmedValue` method that raises notifications for `BoolValue`, `IntValue`, `SelectedOptionIndex`, and `SelectedOption`. Avoid remove-and-append, which also reorders settings.

---

### [High] Event-loop lifetime, disconnection, and reconnection are unmanaged

**Evidence**

- Initialization and polling use `async void`: [`LobbyViewModel.cs:272`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/ViewModels/LobbyViewModel.cs:272), [`LobbyViewModel.cs:348`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/ViewModels/LobbyViewModel.cs:348).
- `LobbyViewModel` has no disposal or cancellation contract.
- Transport polling is fire-and-forget at [`LobbyService.cs:145`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/Services/Playing/LobbyService.cs:145).
- When the stream becomes unavailable, polling simply exits at [`MultiplayerLobby.cs:169`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/Models/Lobbies/MultiplayerLobby.cs:169). The event channel remains open and the UI receives no disconnected state.
- No snapshot/replay or revision-resume path exists.

**Why it matters**

The UI may remain apparently connected while permanently stale. Recreating a view model for the same lobby starts another channel consumer, causing events to be split between old and new views. Delayed/download callbacks can also reach inactive view models.

**Correction**

- Make initialization return `Task`.
- Store and supervise the event-loop task.
- Extend `GetNextEvent` with `CancellationToken`.
- Make the view model `IAsyncDisposable`, with cancellation owned by the navigation/view-model lifetime.
- Expose `ConnectionState` and use it in command `CanExecute`.
- On reconnect, fetch a snapshot or resume from a known revision before processing new live events.

---

### [High] Stale and out-of-order events are not rejected

**Evidence**

`LobbyEvent` contains only type and argument at [`LobbyEvent.cs:29`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/Models/Lobbies/LobbyEvent.cs:29). Company downloads are started without awaiting at [`MultiplayerLobby.cs:212`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/Models/Lobbies/MultiplayerLobby.cs:212), and completion does not confirm that the slot still references the requested company.

**Why it matters**

If a player changes company twice, download A can complete after download B and overwrite the presentation for the newer selection. Combined with the current feedback-loop bug, the stale completion can even send company A back to the server.

**Correction**

At minimum, carry `companyId` in the completion event and apply it only if the current slot still has that ID. Prefer a per-slot revision and cancel superseded downloads. Server commands that can be retried should carry an operation ID and expected aggregate/slot revision.

---

### [Medium] Failure state is hidden and chat drafts are lost

**Evidence**

- [`LobbyViewModel.cs:503`](/E:/coh2_battlegrounds/v2/Battlegrounds.Client/Battlegrounds/ViewModels/LobbyViewModel.cs:503) clears chat text before awaiting the send.
- `SystemError` events fall through the default event case.
- No command-specific pending, rejection, transport-error, or disconnected state is exposed.

**Correction**

Clear chat only after successful sending, or restore the draft on failure. Handle `SystemError` distinctly from transport failures and server validation errors. Bind visible error/pending state and disable incompatible operations while pending.

## 3. Feedback-loop analysis

- Can a server-applied change produce an outbound command? **Yes**, through the downloaded-company branch.
- Can controls react to programmatic updates? **Yes**; the selection/value controls are `TwoWay`, and their setters treat changes as intent.
- Is origin reliably distinguished? **No.** Map events bypass the setter manually, while other state uses replacement or setter paths inconsistently.
- Are duplicate/stale messages handled? **No revisions, sequence numbers, or operation IDs are visible.**
- Is suppression used? **No formal suppression scope.** Direct backing-field assignments provide localized bypasses, but the pattern is inconsistent and already fails for companies.

## 4. Threading analysis

The event loop will probably resume on the WPF dispatcher today if the view model is constructed on the UI thread, because its awaits capture the current synchronization context. That is an implicit assumption, not a contract.

All changes to `ChatMessages`, setting collections, slot collections, overlays, and `PropertyChanged` should be explicitly dispatched through an injected dispatcher abstraction. The transport and download code can execute independently and mutate the lobby mirror concurrently. No blocking `.Wait()` or `.Result` usage was found.

## 5. Lifecycle analysis

- `LobbyService`/`MultiplayerLobby` own the transport.
- `LobbyViewModel` starts its own channel consumer but has no corresponding cancellation/disposal.
- Back-button navigation eventually disposes the lobby, but arbitrary content replacement or view recreation is not guarded.
- Reconnection is absent; stream loss does not fetch a snapshot or close/signal the consumer.
- Delayed and download callbacks can outlive the visible page.

## 6. Corrected flow

```text
User edits a local draft
  -> explicit async command
  -> immutable request + operation ID + expected revision
  -> server validates and applies
  -> versioned server event
  -> lobby synchronization service rejects stale/duplicate events
  -> dispatcher
  -> ApplyConfirmedState (no network calls)
  -> WPF binding refresh

Reconnect
  -> authenticate
  -> fetch snapshot/resume from revision
  -> apply snapshot on dispatcher
  -> resume versioned live events
```

## 7. Prioritized corrections

1. Remove the company feedback loop and every outbound side effect from `AvailableCompanies`.
2. Stop initialization from unconditionally overwriting the selected company.
3. Normalize the `TeamUpdated` payload contract so all remote slot updates reach the correct team.
4. Separate confirmed state, drafts, and explicit async commands for maps, settings, companies, and AI difficulty.
5. Add cancellable, supervised initialization/event loops with dispatcher marshalling and disposal.
6. Add connection state plus snapshot/revision-based reconnection.
7. Add per-slot revisions/company-ID validation and cancel superseded downloads.
8. Make setting items observable and apply server values without replacing/reordering them.
9. Add regression tests for:
   - downloaded-company events sending zero `SetCompany` calls;
   - reading `AvailableCompanies` sending zero calls;
   - integer slot updates refreshing the expected team;
   - stale download completion being ignored;
   - server setting updates refreshing controls without echo;
   - disposal preventing later UI mutation;
   - reconnection restoring a snapshot.

The filtered `LobbyViewModel` unit suite currently passes: 88/88. However, it intentionally documents the integer-team discrepancy and does not test the company-valued download event that triggers the feedback loop. No source files were changed.
