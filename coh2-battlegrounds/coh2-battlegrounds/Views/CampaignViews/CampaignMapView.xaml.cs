using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Battlegrounds;
using Battlegrounds.Campaigns;
using Battlegrounds.Campaigns.Controller;
using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Gfx;
using Battlegrounds.Util.Coroutines;

using BattlegroundsApp.Resources;
using BattlegroundsApp.Utilities;
using BattlegroundsApp.Views.CampaignViews.Models;

using static Battlegrounds.BattlegroundsInstance;

namespace BattlegroundsApp.Views.CampaignViews {
    
    /// <summary>
    /// Interaction logic for CampaignMapView.xaml
    /// </summary>
    public partial class CampaignMapView : ViewState, INotifyPropertyChanged {

        private List<CampaignUnitFormationModel> m_formationViews;
        private Dictionary<string, ImageSource> m_graphics;

        public CampaignUnitSelectionModel Selection { get; }

        public ICampaignController Controller { get; }

        public ImageSource CampaignMapImage { get; }

        public double CampaignMapWidth => this.CampaignMapImage.Width;

        public double CampaignMapHeight => this.CampaignMapImage.Height;

        public string CampaignDate => this.Controller.Campaign.Turn.Date;

        public Visibility CampaignDialogVisible { get; set; } = Visibility.Collapsed;

        public event PropertyChangedEventHandler PropertyChanged;

        public CampaignMapView(ICampaignController controller) {

            // Assign controller
            this.Controller = controller;

            // Init list
            this.m_formationViews = new List<CampaignUnitFormationModel>();

            // Load graphics
            this.InitializeGrapihcs();

            // Init components
            this.InitializeComponent();

            // Init data
            this.Selection = new CampaignUnitSelectionModel();
            this.CampaignMapImage = PngImageSource.FromMemory(controller.Campaign.PlayMap.RawImageData);
            this.CreateNodeNetwork();
            this.RefreshDisplayedFormations();

            // Hide chat control if singleplayer
            if (controller.IsSingleplayer) {
                this.CampaignChat.Visibility = Visibility.Collapsed;
                // TODO: Expand selection view
            } else {

            }

        }

        private void InitializeGrapihcs() {

            // Create graphics dictionary
            this.m_graphics = new Dictionary<string, ImageSource>();

            // For each GFX map
            this.Controller.Campaign.GfxMaps.ForEach(x => {

                // Loop over all resources
                x.Resources.ForEach(id => {

                    // Get the resource
                    GfxResource resource = x.GetResource(id);
                    var stream = resource.Open();

                    // Decode and store the image
                    this.m_graphics[id] = new PngBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];

                });

            });

        }

        public override void StateOnFocus() {}

        public override void StateOnLostFocus() {}

        private void CreateNodeNetwork() {

            // Loop through all nodes
            this.Controller.Campaign.PlayMap.EachNode(n => {

                // Create figure and add to canvas
                Ellipse node = new Ellipse {
                    Width = 32,
                    Height = 32,
                    Fill = n.Owner == CampaignArmyTeam.TEAM_ALLIES ? Brushes.Red : (n.Owner == CampaignArmyTeam.TEAM_AXIS ? Brushes.Green : Brushes.Gray),
                    Tag = n,
                };

                // Add events
                node.MouseDown += this.NodeClicked;

                // Add node
                this.CampaignMapCanvas.Children.Add(node);

                // Set visual
                n.VisualNode = node;
                n.NodeChange += this.NodePropertyChanged;

                // Set position of node
                SetPosition(node, n.U * this.CampaignMapWidth - (32.0 / 2.0), n.V * this.CampaignMapHeight - (32.0 / 2.0));

            });

            // Loop through all transitions
            this.Controller.Campaign.PlayMap.EachTransition(t => {

                // Create line object
                Line line = new Line {
                    X1 = t.From.U * this.CampaignMapWidth,
                    Y1 = t.From.V * this.CampaignMapHeight,
                    X2 = t.To.U * this.CampaignMapWidth,
                    Y2 = t.To.V * this.CampaignMapHeight,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.75,
                    Tag = t
                };

                // Add line
                this.CampaignMapCanvas.Children.Add(line);

                // Set Z-index
                line.SetValue(Panel.ZIndexProperty, 99);

            });

        }

        private void NodePropertyChanged(CampaignMapNode self, CampaignMapNodeProperty property, object value) {
            switch (property) {
                case CampaignMapNodeProperty.Owner:
                    CampaignArmyTeam team = (CampaignArmyTeam)value;
                    (self.VisualNode as Ellipse).Fill = team == CampaignArmyTeam.TEAM_ALLIES ? Brushes.Red : (team == CampaignArmyTeam.TEAM_AXIS ? Brushes.Green : Brushes.Gray);
                    break;
                default:
                    break;
            }
        }

        private void RefreshDisplayedFormations() {

            // Clear all formations
            this.ClearFormations();

            // Loop through all formations and display them
            this.Controller.Campaign.PlayMap.EachFormation(f => {

                // Banner file
                string banner_element = $"{f.Army}_banner";

                // Create space for UI element (default to null)
                UIElement displayElement = null;

                // If it exists, use an image, otherwise some other default pinkish shape
                if (this.m_graphics.TryGetValue(banner_element, out ImageSource img)) {

                    // Create image
                    displayElement = new Image() {
                        Source = img,
                        Width = 24,
                        Height = 32,
                        Tag = f,
                    };

                    // Add handler(s)
                    displayElement.MouseDown += this.BannerClicked;

                    // Add to canvas
                    this.CampaignMapCanvas.Children.Add(displayElement);

                    // Set the display position
                    SetPosition(displayElement, f.Node.U * this.CampaignMapWidth, f.Node.V * this.CampaignMapHeight);

                }

                // Create container type
                CampaignUnitFormationModel cufv = new CampaignUnitFormationModel(displayElement, f);

                this.m_formationViews.Add(cufv);

            });

        }

        private void ClearFormations() {
            foreach (var form in this.m_formationViews) {
                if (form.Element is not null) {
                    this.CampaignMapCanvas.Children.Remove(form.Element);
                }
            }
            this.m_formationViews.Clear();
        }

        private void NodeClicked(object sender, MouseButtonEventArgs e) {
            if (sender is Ellipse ellipse && ellipse.Tag is CampaignMapNode node) {
                if (e.LeftButton == MouseButtonState.Pressed) {
                    this.Selection.Select(node.Occupants.Select(x => this.FromFormation(x)));
                } else if (e.RightButton == MouseButtonState.Pressed) {
                    this.NodeRightClicked(node);
                }
            }
        }

        private void NodeRightClicked(CampaignMapNode node) {

            if (this.Selection.Size > 0) {
                if (this.Selection.Shares(x => x.Formation.Node)) {
                    if (this.Selection.Filter(x => x.Formation.CanMove) > 0) {
                        if (this.Selection.All(x => x.Formation.Team == node.Owner)) {
                            this.MoveSelection(node);
                        } else {
                            if (node.Occupants.Count == 0) {
                                this.MoveSelection(node);
                            } else {
                                this.MoveSelectionIntoHostileTerritory(node);
                            }
                        }
                    }
                }
            }

        }

        private void BannerClicked(object sender, MouseButtonEventArgs e) {
            if (sender is Image img && img.Tag is Formation formation) {
                if (this.FromFormation(formation) is CampaignUnitFormationModel model) {
                    if (e.LeftButton == MouseButtonState.Pressed) {
                        Keyboard.IsKeyDown(Key.LeftShift).IfTrue().Then(() => this.Selection.AddToSelection(model)).Else(() => this.Selection.Select(model));
                    }
                }
            }
        }

        private CampaignUnitFormationModel FromFormation(Formation formation) => this.m_formationViews.FirstOrDefault(x => x.Formation == formation);

        private static void SetPosition(UIElement element, double x, double y) {
            element.SetValue(Canvas.LeftProperty, x);
            element.SetValue(Canvas.TopProperty, y);
            element.SetValue(Panel.ZIndexProperty, 100);
        }

        private static void GotoPosition(UIElement element, double x, double y) {

            DoubleAnimation xAnim = new DoubleAnimation((double)element.GetValue(Canvas.LeftProperty), x, new Duration(TimeSpan.FromSeconds(2))) {
                 RepeatBehavior = new RepeatBehavior(1),
            };
            xAnim.Freeze();

            DoubleAnimation yAnim = new DoubleAnimation((double)element.GetValue(Canvas.TopProperty), y, new Duration(TimeSpan.FromSeconds(2))) {
                RepeatBehavior = new RepeatBehavior(1),
            };
            yAnim.Freeze();

            element.BeginAnimation(Canvas.LeftProperty, xAnim, HandoffBehavior.Compose);
            element.BeginAnimation(Canvas.TopProperty, yAnim, HandoffBehavior.Compose);

        }

        private void EndTurnBttn_Click(object sender, RoutedEventArgs e) {
            if (!this.Controller.EndTurn()) {
                this.Controller.EndCampaign();
            }
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CampaignDate)));
        }

        /// <summary>
        /// Move the entire selection to node
        /// </summary>
        private void MoveSelection(CampaignMapNode node) {
            this.Selection.InvokeEach(x => {
                if (this.Controller.Campaign.PlayMap.SetPath(x.Formation.Node, node, x.Formation)) {
                    this.MoveFormation(x);
                }
            });
        }

        /// <summary>
        /// Move the entire selection (with same origin) into a hostile node
        /// </summary>
        private void MoveSelectionIntoHostileTerritory(CampaignMapNode node) {
            var first = this.Selection.First;
            if (this.Controller.Campaign.PlayMap.SetPath(first.Formation.Node, node, first.Formation)) {
                this.Selection.InvokeEach(x => x.IfTrue(y => y.Formation != first.Formation).Then(z => z.Formation.SetNodeDestinations(first.Formation.GetPath())));
                if (first.Formation.Destination == node) {
                    SelectionAttackNode(node);
                } else {
                    this.Selection.InvokeEach(MoveFormation);
                }
            }
        }

        /// <summary>
        /// This will move a formation to its next destination
        /// </summary>
        private void MoveFormation(CampaignUnitFormationModel formationModel) {
            
            // Make sure there's a destination
            if (formationModel.Formation.Destination is CampaignMapNode node) {
                
                // Move in map
                this.Controller.Campaign.PlayMap.MoveTo(formationModel.Formation, node);

                // Show visually
                GotoPosition(formationModel.Element, node.U * this.CampaignMapWidth, node.V * this.CampaignMapHeight);

                // Check if we can move
                formationModel.Formation.CanMove = false;
            
            }

        }

        /// <summary>
        /// This will display the attacking dialog
        /// </summary>
        private void SelectionAttackNode(CampaignMapNode node) {

            // Get attackers, lock selection
            List<Formation> attackers = this.Selection.Get();
            this.Selection.Lock();

            // Create engagement view
            CampaignEngagementDialogView engagementDialogView = new CampaignEngagementDialogView() {
                Header = Localize.GetString("CampaignView_AttackString", node.NodeName),
                Attackers = Localize.GetEnum(attackers.First().Team),
                Defenders = Localize.GetEnum(node.Owner),
                MapNode = node,
            };

            // Subscribe to events
            engagementDialogView.SubscribeToDialogEvent(CampaignEngagementDialogView.WITHDRAW, this.WithdrawAttack);
            engagementDialogView.SubscribeToDialogEvent(CampaignEngagementDialogView.AUTO, this.AutoAttack);
            engagementDialogView.SubscribeToDialogEvent(CampaignEngagementDialogView.ENGAGE, this.EngageAttack);

            // Set formations etc.
            engagementDialogView.SetAttackingFormations(attackers);
            engagementDialogView.SetupMatchData(this.Controller.Campaign);

            // Show dialog
            this.ShowCampaignDialog(engagementDialogView);

        }

        private void WithdrawAttack(CampaignDialogView dialogView) {
            this.HideCampaignDialog();
        }

        private void AutoAttack(CampaignDialogView dialogView) {
            this.HideCampaignDialog();
        }

        private void EngageAttack(CampaignDialogView dialogView) {
            this.HideCampaignDialog();
            CampaignEngagementDialogView cedv = dialogView as CampaignEngagementDialogView;

            CampaignEngagementData data = new CampaignEngagementData();
            this.SetupEngagementData(ref data, cedv);
            this.ApplyEngagementData(true, ref data, cedv);

            if (this.Controller is SingleplayerCampaign) {
                
                this.Controller.GenerateAIEngagementSetup(ref data, true, cedv.MapNode.Occupants.Count, cedv.MapNode.Occupants.ToArray());
                this.Controller.ZipPlayerData(ref data);

                var matchController = this.Controller.Engage(data);
                matchController.Control();

                ICampaignController.HandleEngagement(matchController, data, this.OnEngagementOver);

                this.ShowBattleStatusDialog();

            } else {
                // TODO: Show defence view
            }

        }

        private void ShowBattleStatusDialog() {
            // TODO: Show battle status
        }

        private void SetupEngagementData(ref CampaignEngagementData engagementData, CampaignEngagementDialogView view) {

            // Setup base data
            engagementData.node = view.MapNode;
            engagementData.scenario = view.EngagementScenario;
            engagementData.allSquads = new List<Squad>();
            engagementData.initialSquads = new List<Squad>();
            engagementData.attackers = view.Attackers.ToLower() == "allies" ? CampaignArmyTeam.TEAM_ALLIES : CampaignArmyTeam.TEAM_AXIS;
            engagementData.defenders = engagementData.attackers == CampaignArmyTeam.TEAM_AXIS ? CampaignArmyTeam.TEAM_ALLIES : CampaignArmyTeam.TEAM_AXIS;
            engagementData.attackingFaction = view.AttackingRegimentalPool.First().Regiment.ElementOf.EleemntOf.Faction;
            engagementData.defendingFaction = view.MapNode.Occupants.First().Regiments.First().ElementOf.EleemntOf.Faction;
            engagementData.attackingFormations = this.Selection.Get();
            engagementData.defendingFormations = view.MapNode.Occupants;

            // TODO: Map players etc.

        }

        private void ApplyEngagementData(bool isAttackerData, ref CampaignEngagementData engagementData, CampaignEngagementDialogView view) {
            if (isAttackerData) {
                
                // Set company data
                engagementData.attackingCompanyUnits = new List<Squad>[view.Players];
                engagementData.attackingDifficulties = new AIDifficulty[view.Players];
                for (int i = 0; i < view.Players; i++) {
                    engagementData.attackingDifficulties[i] = AIDifficulty.Human;
                    switch (i) {
                        case 0:
                            engagementData.attackingCompanyUnits[i] = view.Player1Units.Select(x => x.Squad).ToList();
                            break;
                        case 1:
                            engagementData.attackingCompanyUnits[i] = view.Player2Units.Select(x => x.Squad).ToList();
                            break;
                        case 2:
                            engagementData.attackingCompanyUnits[i] = view.Player3Units.Select(x => x.Squad).ToList();
                            break;
                        case 3:
                            engagementData.attackingCompanyUnits[i] = view.Player4Units.Select(x => x.Squad).ToList();
                            break;
                        default:
                            break;
                    }
                }

            } else {

                // Set company data
                engagementData.defendingCompanyUnits = new List<Squad>[view.Players];
                engagementData.defendingDifficulties = new AIDifficulty[view.Players];
                for (int i = 0; i < view.Players; i++) {
                    engagementData.defendingDifficulties[i] = AIDifficulty.Human;
                    switch (i) {
                        case 0:
                            engagementData.defendingCompanyUnits[i] = view.Player1Units.Select(x => x.Squad).ToList();
                            break;
                        case 1:
                            engagementData.defendingCompanyUnits[i] = view.Player2Units.Select(x => x.Squad).ToList();
                            break;
                        case 2:
                            engagementData.defendingCompanyUnits[i] = view.Player3Units.Select(x => x.Squad).ToList();
                            break;
                        case 3:
                            engagementData.defendingCompanyUnits[i] = view.Player4Units.Select(x => x.Squad).ToList();
                            break;
                        default:
                            break;
                    }
                }

            }
        }

        private void OnEngagementOver(CampaignMapNode mapNode, bool attackSuccessful) {

            // TODO: Hide "Playing battle"

            // Start coroutine
            Coroutine.StartCoroutine(this.NodeLostVisuals(mapNode, attackSuccessful), (GUIThreadDispatcher)this.Dispatcher);

            // Update logs etc.

        }

        private IEnumerator NodeLostVisuals(CampaignMapNode mapNode, bool attackSuccessful) {

            // Get attackers from locked selection
            var attackers = this.Selection.Get();

            // Destroy low length formations
            static bool DestroyLowStrengthFormations(Formation formation) {
                if (formation.CalculateStrength() <= 0.025f) {
                    // TODO: Kill attacker visually (This may need to be in the outer method for coroutine functionality)
                    return true;
                } else {
                    return false;
                }
            }

            // Remove all dead attackers and defenders
            int lostAttackers = attackers.RemoveAll(DestroyLowStrengthFormations);
            int lostDefenders = mapNode.Occupants.RemoveAll(DestroyLowStrengthFormations);

            // Log event
            Trace.WriteLine($"Attackers lost {lostAttackers} formations and defenders lost {lostDefenders}.", nameof(CampaignMapView));

            // It the attack was successful
            if (attackSuccessful) {

                // Get neighbouring nodes of current ally
                var nodes = this.Controller.Campaign.PlayMap.GetNodeNeighbours(mapNode, x => x.Occupants.Count < x.OccupantCapacity && x.Owner == mapNode.Owner);
                int nodeIndex = 0;

                if (nodes.Count == 0) {

                    // TODO: Destroy all occupants (nowhere to run to)

                } else {

                    // Tell all current occupants to leave
                    var itter = mapNode.Occupants.GetSafeEnumerator();
                    while (itter.MoveNext()) {
                        itter.Current.SetNodeDestinations(new List<CampaignMapNode>() { nodes[nodeIndex] });
                        this.MoveFormation(this.FromFormation(itter.Current));
                        nodeIndex++;
                        if (nodeIndex > nodes.Count) {
                            nodeIndex = 0;
                        }
                        yield return new WaitTimespan(TimeSpan.FromSeconds(2));
                    }

                }

                // Move in attackers
                foreach (var attacker in attackers) {
                    this.MoveFormation(this.FromFormation(attacker));
                    yield return new WaitTimespan(TimeSpan.FromSeconds(2));
                }

            }

            // Unlock selection
            this.Selection.Unlock();

        }

        private void LeaveAndSaveButton_Click(object sender, RoutedEventArgs e) {

        }

        private void ShowCampaignDialog(CampaignDialogView dialogView) {
            
            // Set background visibility
            CampaignDialogVisible = Visibility.Visible;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CampaignDialogVisible)));
            
            // Show Dialog
            dialogView.ShowDialogView(this.CampaignMapCanvas);

        }

        private void HideCampaignDialog() {

            // Set background visibility
            CampaignDialogVisible = Visibility.Collapsed;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CampaignDialogVisible)));

        }

    }

}
