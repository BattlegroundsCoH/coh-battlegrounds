# WPF MVVM Online-State Review Guide for LLMs

## Purpose

Use this guide when reviewing a WPF MVVM implementation in which:

- User actions produce commands or events that are sent to a server.
- The server is authoritative for some or all application state.
- Server messages can update the local ViewModel and therefore the WPF controls.
- A server-applied update must not accidentally be interpreted as a new user action and sent back to the server.
- Connections may be asynchronous, delayed, interrupted, duplicated, or reordered.

The goal of the review is to identify architectural, correctness, threading, lifecycle, and maintainability problems, and then offer concrete corrections.

---

## Reviewer Role

Act as a senior .NET/WPF reviewer.

Analyze the supplied implementation rather than replacing it with an unrelated architecture. Preserve the existing design where practical, but recommend structural changes when the current design cannot reliably distinguish user intent from synchronized state.

Do not merely list generic MVVM advice. Tie every finding to specific code, behavior, or an explicitly stated missing safeguard.

---

## Core Architectural Rule

> Commands describe what the user wants. ViewModel properties describe the state the application currently believes to be true.

A property update caused by a server message must not automatically create a new outbound command.

The preferred flow is:

```text
User interaction
    -> View command
    -> ViewModel/application command
    -> server request
    -> server validation and state transition
    -> server state-change event
    -> local synchronization service
    -> ViewModel property update
    -> WPF binding updates the control
```

The final binding update must not restart the flow.

---

# Review Procedure

Perform the review in this order.

## 1. Identify State and Intent

Determine which members represent:

- Editable UI drafts.
- User intent.
- Pending requests.
- Server-confirmed state.
- Derived presentation state.
- Connection state.
- Error state.

Flag cases where one property is ambiguously serving several of these purposes.

Example of an ambiguous property:

```csharp
public bool IsEnabled
{
    get => _isEnabled;
    set
    {
        _isEnabled = value;
        SendChangeToServer(value);
        OnPropertyChanged();
    }
}
```

This setter cannot distinguish between a local edit and a server-applied update.

## 2. Trace Every Outbound Operation

For each server call, determine:

- What user action triggers it.
- Whether a programmatic property update can also trigger it.
- Whether it can execute more than once.
- Whether it is awaited.
- Whether it supports cancellation.
- Whether failure is surfaced.
- Whether retries can duplicate the operation.
- Whether the operation has an identity or idempotency mechanism.

## 3. Trace Every Inbound Update

For each server event or response, determine:

- Which thread invokes the callback.
- How execution is marshalled to the UI thread.
- Which ViewModel properties or collections are modified.
- Whether stale or duplicate messages are rejected.
- Whether the update can trigger another outbound operation.
- Whether the associated ViewModel is still alive and active.
- Whether ordering is assumed but not guaranteed.

## 4. Inspect WPF Binding Behavior

Check:

- `Mode`.
- `UpdateSourceTrigger`.
- Validation behavior.
- Whether a control event fires for both user and programmatic changes.
- Whether commands are used for actions.
- Whether code-behind is forwarding behavior or owning application logic.
- Whether bindings can cause excessive network traffic, especially for text boxes, sliders, and selection controls.

## 5. Inspect Lifecycle and Reconnection

Check:

- Connection startup and shutdown.
- Event subscription and unsubscription.
- ViewModel disposal.
- Cancellation of in-flight work.
- Reconnection behavior.
- State snapshot or replay after reconnection.
- Handling of events received while a ViewModel is inactive.
- Duplicate subscriptions after navigation or reconnect.

## 6. Propose Minimal Corrections

Prefer the smallest correction that establishes clear state ownership and safe synchronization.

When a local patch is insufficient, explain why and propose a structural correction.

---

# Do's

## Separate User Intent from State Mutation

Do invoke server operations from explicit commands or application methods.

```csharp
[RelayCommand]
private Task SetEnabledAsync(bool enabled)
{
    return _server.SendAsync(new SetEnabledCommand(enabled));
}
```

Do allow the property to represent state without automatically causing I/O.

```csharp
[ObservableProperty]
private bool _enabled;
```

Do provide a separate method for applying server state.

```csharp
public void Apply(EnabledChanged message)
{
    Enabled = message.Enabled;
}
```

## Treat the Server as Authoritative When Appropriate

Do clearly state whether the UI is:

- Optimistic.
- Server-confirmed.
- Based on a local draft plus confirmed server state.

Do reconcile optimistic local state with the eventual server result.

Do restore or correct the UI if the server rejects or transforms the requested value.

## Use Operation IDs and Revisions

Do recommend an operation ID for commands that may be retried.

```csharp
public sealed record SetEnabledCommand(
    Guid OperationId,
    long ExpectedRevision,
    bool Enabled);
```

Do recommend a monotonically increasing revision, sequence number, timestamp with defined semantics, or another explicit ordering mechanism for inbound state.

```csharp
if (message.Revision <= _lastAppliedRevision)
    return;
```

Do maintain revisions at the correct scope. A revision may belong to:

- A single entity.
- A document.
- A shared aggregate.
- A complete synchronized session.

Do not assume that one global revision is always correct.

## Marshal UI Changes to the Dispatcher

Do assume that transport callbacks may run outside the WPF UI thread.

```csharp
await _dispatcher.InvokeAsync(() =>
{
    Enabled = message.Enabled;
});
```

Do apply the same care to:

- `ObservableCollection<T>`.
- `CollectionView`.
- Dependency objects.
- UI-bound validation state.

Do avoid blocking dispatcher calls when an asynchronous dispatch is sufficient.

## Use Async Commands Correctly

Do return `Task` from asynchronous command handlers.

```csharp
[RelayCommand]
private async Task SaveAsync(CancellationToken cancellationToken)
{
    await _service.SaveAsync(cancellationToken);
}
```

Do surface pending and failure state.

```csharp
public bool IsSaving { get; }
public string? SaveError { get; }
```

Do prevent accidental concurrent execution when the operation is not reentrant.

Do propagate cancellation when the ViewModel is closed or replaced.

## Queue or Coalesce High-Frequency Changes

Do consider a queue, channel, debounce, throttle, or explicit commit action for:

- `TextChanged`.
- Slider movement.
- Drag operations.
- Rapid selection changes.
- Repeated toggles.

Do separate editing state from committed state when the user is expected to type or manipulate a control continuously.

```csharp
public string DraftName { get; set; }
public string ConfirmedName { get; private set; }
```

Do send on a deliberate action such as Save, Enter, loss of focus, or a debounce interval when that matches the product requirements.

## Use Suppression Only as a Localized Fallback

A suppression scope may be acceptable when a third-party control exposes only a change event and cannot distinguish user-generated changes.

Do make suppression:

- Explicit.
- Narrowly scoped.
- Exception-safe.
- UI-thread-confined.
- Nesting-safe.

```csharp
private int _serverUpdateDepth;

private bool IsApplyingServerUpdate => _serverUpdateDepth > 0;

private void ApplyServerUpdate(Action update)
{
    _serverUpdateDepth++;

    try
    {
        update();
    }
    finally
    {
        _serverUpdateDepth--;
    }
}
```

Do prefer a counter or disposable scope over a simple boolean when nesting is possible.

Do explain that suppression is not a substitute for command/state separation.

## Handle Connection Loss Explicitly

Do define what happens to outbound commands while disconnected:

- Reject immediately.
- Queue locally.
- Persist to an outbox.
- Allow only idempotent retries.
- Require manual retry.

Do define what happens after reconnect:

```text
Reconnect
    -> authenticate
    -> fetch snapshot or resume from revision
    -> apply authoritative state
    -> resume live events
```

Do ensure live events cannot overtake snapshot application without a defined ordering strategy.

## Manage Subscriptions and Object Lifetimes

Do ensure server event handlers are unsubscribed.

Do use `IDisposable` or `IAsyncDisposable` when the ViewModel owns subscriptions, connections, timers, or cancellation sources.

Do check for repeated subscriptions during:

- View recreation.
- Navigation.
- Dependency injection scope creation.
- Reconnection.
- Data-context replacement.

Do consider weak-event patterns only when they genuinely fit; do not use them to avoid proper ownership.

## Keep Transport Concerns Outside the View

Do keep SignalR, WebSocket, gRPC, or HTTP details out of controls and code-behind.

Do place transport behavior in a service, synchronization coordinator, gateway, or application layer.

Do inject an abstraction into the ViewModel where practical.

```csharp
public interface ISettingsSession
{
    Task SendAsync(SetEnabledCommand command, CancellationToken cancellationToken);
    IAsyncEnumerable<SettingsEvent> ReadEventsAsync(
        CancellationToken cancellationToken);
}
```

## Make State Transitions Testable

Do recommend tests that verify:

1. A user command sends exactly one request.
2. A server update changes the ViewModel.
3. A server update does not send a request.
4. Duplicate server events are ignored.
5. Stale revisions are ignored.
6. A rejected optimistic update is reconciled.
7. Disposal prevents later callbacks from mutating the ViewModel.
8. Reconnection restores authoritative state.
9. High-frequency edits are coalesced as intended.
10. Exceptions do not leave suppression or pending flags stuck.

---

# Don'ts

## Do Not Perform Network I/O in Ordinary Property Setters

Avoid:

```csharp
public string Name
{
    get => _name;
    set
    {
        if (SetProperty(ref _name, value))
            _ = _server.SetNameAsync(value);
    }
}
```

Problems include:

- Server-applied values are transmitted again.
- Exceptions may be lost.
- Cancellation is unclear.
- Ordering is unclear.
- Rapid edits produce uncontrolled traffic.
- Testing state mutation requires network behavior.
- Property assignment gains hidden side effects.

Recommend an explicit command, commit operation, or carefully designed change pipeline instead.

## Do Not Use `async void` Except for Actual UI Event Handlers

Avoid `async void` ViewModel methods and service callbacks.

Problems include:

- Exceptions bypass ordinary task handling.
- Callers cannot await completion.
- Cancellation and ordering are difficult to coordinate.
- Tests cannot reliably observe completion.

When an actual WPF event handler must be `async void`, keep it small and delegate to an awaitable method.

## Do Not Update UI-Bound State from Arbitrary Threads

Avoid directly modifying ViewModel collections or UI-bound dependency objects from transport callbacks.

Do not assume that `INotifyPropertyChanged` makes cross-thread updates safe.

Do not use `Dispatcher.Invoke` indiscriminately if it may deadlock or unnecessarily block a background callback.

## Do Not Treat `PropertyChanged` as a User-Input Event

`PropertyChanged` means that a property changed. It does not identify why it changed.

Do not infer that:

```csharp
PropertyChanged += (_, e) => SendToServer(e.PropertyName);
```

represents user intent.

Changes may originate from:

- Server synchronization.
- Initialization.
- Validation.
- Undo or redo.
- Navigation.
- Calculated state.
- Deserialization.
- Tests.
- Another local component.

## Do Not Rely Only on Equality Checks to Prevent Loops

This is insufficient:

```csharp
if (_enabled == value)
    return;
```

A server may:

- Echo the same value after a local optimistic update.
- Normalize a value.
- Reject a value and restore the old one.
- Send a newer event with an equal field value but different related state.
- Repeat an event after reconnection.

Equality checks are useful for avoiding redundant notifications, but they are not an operation-origin or ordering protocol.

## Do Not Ignore Duplicate or Out-of-Order Messages

Do not assume a network transport, reconnect sequence, retry layer, or message broker provides exactly-once ordered delivery unless that guarantee is explicit and correctly scoped.

Flag code that blindly applies every message.

Recommend IDs, revisions, sequence numbers, or idempotent state replacement.

## Do Not Block on Tasks

Avoid:

```csharp
_service.SendAsync(command).Wait();
_service.SendAsync(command).Result;
```

This may deadlock the WPF dispatcher and will block the UI.

Use `await`.

## Do Not Put Business Logic in Code-Behind

Code-behind may:

- Translate a WPF-specific event into a command.
- Handle view-only behavior.
- Manage focus, animations, or purely visual details.

It should not:

- Decide server commands.
- Own authoritative state.
- Implement retry policy.
- Mutate domain state directly.
- Coordinate synchronization.

## Do Not Recreate Connections Per User Action

Avoid creating a SignalR, WebSocket, or gRPC streaming connection every time a button is clicked.

Prefer a connection/session service with an explicit lifecycle.

Ordinary HTTP requests may be per operation, but `HttpClient` should still be managed correctly rather than recreated indiscriminately.

## Do Not Hide Failures

Avoid fire-and-forget operations without deliberate supervision.

Bad:

```csharp
_ = SaveAsync();
```

Acceptable only when routed through a mechanism that observes and reports failures.

Do not leave the UI displaying an optimistic value indefinitely after a failed request.

## Do Not Subscribe with Anonymous Delegates That Cannot Be Removed

Avoid:

```csharp
_server.Changed += (_, message) => Apply(message);
```

when the subscription must later be removed and the delegate is not retained.

Prefer a named handler or a subscription object.

## Do Not Use a Global Boolean Suppression Flag

Avoid:

```csharp
_isUpdating = true;
Value = message.Value;
_isUpdating = false;
```

Problems include:

- Exceptions can leave the flag set.
- Nested updates break the flag.
- Multiple ViewModels can interfere if the flag is static or shared.
- Background-thread access can race.
- It obscures the real distinction between intent and state.

Use an exception-safe local scope only where unavoidable.

## Do Not Assume Reconnection Automatically Restores State

A reconnected socket does not prove that the client received every prior state transition.

Require one of:

- A fresh snapshot.
- Replay from a known revision.
- A protocol that combines snapshot and subsequent events consistently.

---

# WPF-Specific Review Checklist

## Bindings

Check whether:

- `TwoWay` binding is actually required.
- `OneWay` is more appropriate for server-confirmed state.
- `UpdateSourceTrigger=PropertyChanged` causes traffic on every keystroke.
- `LostFocus` or `Explicit` would better represent commit semantics.
- Validation errors remain visible after server rejection.
- Converters contain business logic or side effects.
- Binding fallback values can be mistaken for authoritative state.

## Commands

Check whether:

- The command represents a user action.
- `CanExecute` reflects pending, disconnected, or invalid state.
- Reentrancy is permitted intentionally.
- Parameters are strongly typed or validated.
- The command captures stale values.
- A command can execute after the ViewModel is disposed.

## Collections

Check whether:

- The collection is modified on the UI thread.
- Individual events should be applied incrementally or replaced by a snapshot.
- Selection survives replacement.
- `CollectionViewSource.GetDefaultView` is accessed on the correct thread.
- Bulk updates produce excessive notifications.
- Item identity is stable across synchronization.

## Validation

Check whether validation is:

- Local-only.
- Server-only.
- Split between both.

Do not treat local validation as proof that the server will accept the operation.

Recommend representing server validation errors separately from transport failures.

## Dependency Properties and Custom Controls

For custom controls, check whether:

- Property-changed callbacks incorrectly send network operations.
- Coercion callbacks have side effects.
- Routed events distinguish user interaction from state changes.
- A dependency property change caused by binding is misinterpreted as direct user input.

A custom control should expose a command or a clearly user-originated routed event when an action must be communicated.

---

# Online Synchronization Patterns

## Pattern A: Explicit User Command

Prefer this pattern for buttons, menu items, checkboxes with commands, and deliberate actions.

```csharp
public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _service;

    [ObservableProperty]
    private bool _enabled;

    [RelayCommand]
    private Task RequestEnabledChangeAsync(
        bool enabled,
        CancellationToken cancellationToken)
    {
        return _service.RequestEnabledChangeAsync(
            enabled,
            cancellationToken);
    }

    public void Apply(EnabledChanged message)
    {
        Enabled = message.Enabled;
    }
}
```

The LLM should verify that applying `EnabledChanged` cannot invoke `RequestEnabledChangeAsync`.

## Pattern B: Draft and Commit

Prefer this pattern for text, forms, and multi-field editing.

```csharp
public string DraftName { get; set; } = "";
public string ConfirmedName { get; private set; } = "";

[RelayCommand]
private async Task SaveNameAsync(CancellationToken cancellationToken)
{
    await _service.SetNameAsync(DraftName, cancellationToken);
}

public void Apply(NameChanged message)
{
    ConfirmedName = message.Name;
    DraftName = message.Name;
}
```

The review must consider whether overwriting an active draft is acceptable. In collaborative scenarios, conflict handling may be required.

## Pattern C: Optimistic State with Pending Operation

Prefer this pattern when immediate visual feedback matters.

```csharp
public bool DisplayedEnabled { get; private set; }
public bool ConfirmedEnabled { get; private set; }
public bool IsPending { get; private set; }
public Guid? PendingOperationId { get; private set; }
```

The LLM should require clear behavior for:

- Acceptance.
- Rejection.
- Timeout.
- A conflicting remote change.
- A response arriving after a newer local operation.

## Pattern D: Immediate Continuous Updates

Use only when the product truly requires near-real-time changes.

Require:

- Debouncing or coalescing.
- Sequence IDs or revisions.
- Cancellation of superseded requests where supported.
- A defined final-value guarantee.
- A server reconciliation event.
- No asynchronous work directly in a property setter.

## Pattern E: Local Suppression Scope

Use only when integration constraints prevent a cleaner separation.

```csharp
private int _applyDepth;

private IDisposable BeginServerApply()
{
    _applyDepth++;
    return new DelegateDisposable(() => _applyDepth--);
}

private void OnControlValueChanged(double value)
{
    if (_applyDepth != 0)
        return;

    QueueUserChange(value);
}

private void ApplyServerValue(double value)
{
    using (BeginServerApply())
    {
        ControlValue = value;
    }
}
```

The LLM should label this as a containment technique, not the preferred core architecture.

---

# Findings Severity

Classify every finding.

## Critical

Use when the implementation can cause:

- Infinite or sustained client/server feedback loops.
- UI deadlocks.
- Cross-thread collection failures.
- Corrupted synchronized state.
- Unbounded duplicate operations.
- Security-sensitive actions to repeat.
- Lost updates with no detection.

## High

Use when the implementation can cause:

- Stale state to overwrite newer state.
- Exceptions to disappear.
- Duplicate subscriptions.
- Incorrect reconnection behavior.
- Optimistic UI state to remain permanently incorrect.
- Operations to execute after disposal.

## Medium

Use when the implementation causes:

- Excessive network calls.
- Poor cancellation.
- Weak lifecycle management.
- Difficult testing.
- Mixed responsibilities.
- Avoidable UI stalls.
- Fragile suppression logic.

## Low

Use for:

- Naming.
- Readability.
- Minor binding improvements.
- Small allocation or notification inefficiencies.
- Optional modernization.

Do not label stylistic preferences as critical defects.

---

# Required Review Output

The review should use the following structure.

## 1. Architecture Summary

Briefly explain:

- How user actions currently reach the server.
- How server changes currently reach the UI.
- Where state ownership appears to reside.
- Whether the implementation distinguishes intent from state.

## 2. Findings

For each finding, provide:

### `[Severity] Concise title`

**Evidence**

Quote or identify the relevant class, method, property, binding, or event.

**Why it matters**

Explain the concrete failure mode.

**Correction**

Describe the smallest reliable correction.

**Example**

Provide a focused code example when useful.

Do not provide an enormous replacement implementation for a small issue.

## 3. Feedback-Loop Analysis

Answer explicitly:

- Can a server-applied property change produce a new outbound command?
- Can a control event fire for both user and programmatic updates?
- Is there a reliable origin distinction?
- Are duplicates and stale messages handled?
- Is suppression being used, and is it safe?

## 4. Threading Analysis

Answer explicitly:

- Which callbacks may execute off the UI thread?
- Where is dispatcher marshalling required?
- Are collections and views mutated safely?
- Is there any blocking wait on the dispatcher?

## 5. Lifecycle Analysis

Answer explicitly:

- Who owns the connection?
- Who owns subscriptions?
- How are they disposed?
- What happens during navigation, shutdown, and reconnect?
- Can callbacks reach dead or inactive ViewModels?

## 6. Corrected Flow

Show the corrected message flow as text or a compact diagram.

```text
User action
    -> command
    -> outbound service
    -> server
    -> versioned server event
    -> synchronization service
    -> dispatcher
    -> ViewModel state
    -> binding
```

## 7. Prioritized Corrections

Finish with an ordered list:

1. Required correctness fixes.
2. Threading and lifecycle fixes.
3. Reliability improvements.
4. Maintainability improvements.
5. Optional refinements.

---

# Correction Rules for the LLM

## Preserve Context

- Use the libraries already present unless they are the source of the problem.
- Match the project's existing MVVM framework where possible.
- Do not mix CommunityToolkit.Mvvm, ReactiveUI, Prism, and custom infrastructure without justification.
- Do not invent server guarantees.
- Mark assumptions explicitly.

## Prefer Compilable Code

When offering code:

- Include necessary field types.
- Use valid C# syntax.
- Use `Task`, not pseudo-async code.
- Include cancellation where relevant.
- Avoid APIs not available in the stated target framework.
- Do not claim thread safety without synchronization.
- Do not put network calls in property setters merely to shorten the example.

## Avoid Overengineering

Do not automatically prescribe:

- Event sourcing.
- CQRS infrastructure.
- A message broker.
- Reactive extensions.
- A full offline-first store.
- Distributed transactions.

Recommend these only when the stated requirements justify them.

## Explain Trade-offs

When proposing optimistic updates, explain reconciliation complexity.

When proposing server-confirmed updates, explain latency and responsiveness.

When proposing debouncing, explain that intermediate values may not be sent.

When proposing local queuing, explain duplicate, ordering, persistence, and conflict implications.

---

# Common Review Questions

The LLM should answer these from the supplied code where possible:

1. What exactly is the server-authoritative state?
2. Which actions are true user intents?
3. Can initialization send accidental commands?
4. Can deserialization or snapshot application send accidental commands?
5. Can a server echo create a loop?
6. Can a retry duplicate a non-idempotent action?
7. Can an old response overwrite a newer local request?
8. Can an event arrive after the ViewModel is disposed?
9. Does reconnection fetch missed state?
10. Are transport callbacks marshalled to the UI dispatcher?
11. Are collection mutations safe?
12. Are pending and error states visible?
13. Can users issue conflicting commands concurrently?
14. Does the code distinguish transport failure from server rejection?
15. Is there a test proving that remote updates do not produce outbound messages?

If the code does not provide enough information, state what cannot be verified. Do not silently assume the safe behavior.

---

# Suggested Test Shapes

## Server Update Does Not Echo

```csharp
[Fact]
public void ApplyingServerUpdate_DoesNotSendCommand()
{
    var server = new FakeSettingsService();
    var vm = new SettingsViewModel(server);

    vm.Apply(new EnabledChanged(
        OperationId: Guid.NewGuid(),
        Revision: 1,
        Enabled: true));

    Assert.True(vm.Enabled);
    Assert.Empty(server.SentCommands);
}
```

## User Intent Sends Once

```csharp
[Fact]
public async Task UserCommand_SendsExactlyOnce()
{
    var server = new FakeSettingsService();
    var vm = new SettingsViewModel(server);

    await vm.RequestEnabledChangeCommand.ExecuteAsync(true);

    Assert.Single(server.SentCommands);
}
```

## Stale Event Is Ignored

```csharp
[Fact]
public void OlderRevision_DoesNotOverwriteNewerState()
{
    var vm = CreateViewModel();

    vm.Apply(new EnabledChanged(Guid.NewGuid(), 10, true));
    vm.Apply(new EnabledChanged(Guid.NewGuid(), 9, false));

    Assert.True(vm.Enabled);
}
```

## Suppression Is Restored After Failure

```csharp
[Fact]
public void ApplyScope_IsRestoredWhenUpdateThrows()
{
    var vm = CreateViewModel();

    Assert.Throws<InvalidOperationException>(
        () => vm.ApplyWithTestFailure());

    Assert.False(vm.IsApplyingServerUpdate);
}
```

---

# Compact Do/Don't Summary

| Do | Don't |
|---|---|
| Send user intent through commands | Send network requests from ordinary setters |
| Apply server state through dedicated methods | Treat every property change as user input |
| Use revisions or operation IDs | Assume exactly-once ordered delivery |
| Marshal UI-bound changes to the dispatcher | Update collections from transport threads |
| Await asynchronous operations | Use unobserved fire-and-forget calls |
| Model pending, accepted, and rejected states | Leave optimistic state unreconciled |
| Dispose subscriptions and connections | Allow callbacks to retain dead ViewModels |
| Debounce high-frequency edits | Send every keystroke without a requirement |
| Fetch a snapshot or replay after reconnect | Assume reconnect restores missed state |
| Test that remote updates do not echo | Rely only on equality checks |
| Use scoped suppression only when necessary | Use a shared global suppression boolean |
| Preserve the project's MVVM framework | Replace everything with a fashionable architecture |

---

# Prompt Template

The following can be given directly to an LLM together with the implementation:

```text
Review this WPF MVVM implementation as a senior .NET/WPF engineer.

The application communicates with a server in both directions:
- User actions may send commands to the server.
- Server events may update local ViewModel state and WPF controls.
- A server-applied update must not produce a duplicate outbound command.

Use the attached "WPF MVVM Online-State Review Guide for LLMs."

Analyze:
1. Separation of user intent and application state.
2. Risk of client/server feedback loops.
3. WPF bindings, commands, and control events.
4. UI-thread and Dispatcher correctness.
5. Async, cancellation, failure, and reentrancy behavior.
6. Duplicate, stale, and out-of-order messages.
7. Connection, subscription, navigation, disposal, and reconnection lifecycle.
8. Optimistic versus server-confirmed state.
9. Testability and missing tests.

For every finding, include:
- Severity.
- Specific evidence.
- Concrete failure mode.
- Minimal reliable correction.
- Focused corrected code where useful.

Do not give generic MVVM advice without tying it to the supplied code.
Do not invent server guarantees.
State any assumptions and anything that cannot be verified.
End with a prioritized correction plan.
```
