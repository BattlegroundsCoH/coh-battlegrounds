using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

using Battlegrounds.Campaigns.API;
using Battlegrounds.Campaigns.Controller;
using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Game.Gameplay;

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
        private List<ICampaignPointsNode> m_nodes;
        private CampaignResourceContext m_resourceContext;
        private bool m_hasStarted = false;

        public CampaignUnitSelectionModel Selection { get; }

        public ICampaignController Controller { get; }

        public ImageSource CampaignMapImage { get; }

        public CampaignResourceContext ResourceContext => this.m_resourceContext;

        public double CampaignMapWidth => this.CampaignMapImage.Width;

        public double CampaignMapHeight => this.CampaignMapImage.Height;

        public string CampaignDate => this.Controller.Turn.Date;

        public string CampaignAlliesPoints => $"{this.Controller.GetTeam(CampaignArmyTeam.TEAM_ALLIES).VictoryPoints} Points";

        public string CampaignAxisPoints => $"{this.Controller.GetTeam(CampaignArmyTeam.TEAM_AXIS).VictoryPoints} Points";

        public Visibility CampaignDialogVisible { get; set; } = Visibility.Collapsed;

        public bool CanEndTurn => this.Controller.IsSelfTurn();

        public ObservableCollection<CampaignObjectiveModel> CampaignActiveGoals { get; }

        public ObservableCollection<CampaignUnitReserveModel> CampaignAvailableReserves { get; }

        public GUIThreadDispatcher ThreadDispatcher => (GUIThreadDispatcher)this.Dispatcher;

        public event PropertyChangedEventHandler PropertyChanged;

        public CampaignMapView(ICampaignController controller) {

            // Assign controller
            this.Controller = controller;
            this.Controller.OnAttack += this.OnAttackEvent;
            this.Controller.OnDefend += this.OnDefendEvent;
            this.Controller.OnTurn += this.OnTurnOverEvent;

            // Init lists
            this.CampaignActiveGoals = new ObservableCollection<CampaignObjectiveModel>();
            this.m_formationViews = new List<CampaignUnitFormationModel>();
            this.m_nodes = new List<ICampaignPointsNode>();

            // Load graphics
            this.CampaignMapImage = PngImageSource.FromMemory(controller.Map.RawImageData);
            this.m_resourceContext = new CampaignResourceContext(this.CampaignMapWidth, this.CampaignMapHeight, controller);
            this.m_resourceContext.InitializeGraphics(controller);

            // Init components
            this.InitializeComponent();

            // Init data
            this.Selection = new CampaignUnitSelectionModel();
            this.CreateNodeNetwork();
            this.RefreshDisplayedFormations();

            // Hide chat control if singleplayer
            if (controller.IsSingleplayer) {
                this.CampaignChat.Visibility = Visibility.Collapsed;
                // TODO: Expand selection view
            } else {

            }

            // Self
            var self = this.Controller.GetSelf();

            // Loop over objectives
            var goals = controller.Goals.GetGoals(self.FactionName); // TODO: Fix fake-it code
            foreach (var goal in goals) {
                if (goal.State == CampaignGoalState.Started) {
                    this.CampaignActiveGoals.Add(new CampaignObjectiveModel(goal, this.ResourceContext));
                }
            }

            // Loop over reserves


        }

        private void OnTurnOverEvent() {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CanEndTurn)));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CampaignDate)));
        }

        private CampaignEngagementData? OnAttackEvent(ICampaignFormation[] attackingFormations, ICampaignMapNode node) {

            if (attackingFormations[0].Team != this.Controller.GetSelf().Team.Team) {
                // TODO: Visually show we're being attacked
                // Then maybe a delay here for visuals
                return this.Controller.HandleAttacker(attackingFormations, node);
            } else {

                bool hasResult = false;
                CampaignEngagementData? engagementData = null;

                // Show dialog
                this.Dispatcher.Invoke(() => {

                    // Create engagement view
                    CampaignEngagementDialogView engagementDialogView = new CampaignEngagementDialogView() {
                        Header = Localize.GetString("CampaignView_AttackString", node.NodeName),
                        Attackers = Localize.GetEnum(attackingFormations.First().Team),
                        Defenders = Localize.GetEnum(node.Owner),
                        MapNode = node,
                    };

                    void Attack(CampaignDialogView view) {

                        CampaignEngagementData data = new CampaignEngagementData();
                        this.SetupEngagementData(ref data, view as CampaignEngagementDialogView);
                        this.ApplyEngagementData(true, ref data, view as CampaignEngagementDialogView);

                        engagementData = data;
                        hasResult = true;

                        HideCampaignDialog();

                    }

                    void Withdraw(CampaignDialogView view) {
                        HideCampaignDialog();

                    }

                    // Subscribe to events
                    engagementDialogView.SubscribeToDialogEvent(CampaignEngagementDialogView.WITHDRAW, Withdraw);
                    engagementDialogView.SubscribeToDialogEvent(CampaignEngagementDialogView.ENGAGE, Attack);

                    // Set formations etc.
                    engagementDialogView.SetAttackingFormations(attackingFormations.ToList(), this.Controller.Locale);
                    engagementDialogView.SetupMatchData(this.Controller);

                    this.ShowCampaignDialog(engagementDialogView); 

                });

                // Wait until we've gotten a result
                while (this.CampaignDialogVisible == Visibility.Visible && !hasResult) {
                    Thread.Sleep(1);
                }

                // Return nothing
                return engagementData;

            }

        }

        private CampaignEngagementData OnDefendEvent(CampaignEngagementData engagementData) {

            if (engagementData.defenders != this.Controller.GetSelf().Team.Team) {

                // Handle defence
                this.Controller.HandleDefender(ref engagementData);

            } else {

                bool hasResult = false;

                // Show dialog
                this.Dispatcher.Invoke(() => {

                    // Create engagement view
                    CampaignEngagementDialogView engagementDialogView = new CampaignEngagementDialogView() {
                        Header = Localize.GetString("CampaignView_DefendString", engagementData.node.NodeName),
                        Attackers = Localize.GetEnum(engagementData.defenders), // Reversing roles here
                        Defenders = Localize.GetEnum(engagementData.attackers),
                    };

                    void Attack(CampaignDialogView view) {

                        CampaignEngagementData data = new CampaignEngagementData();
                        this.ApplyEngagementData(false, ref data, view as CampaignEngagementDialogView);

                        engagementData = data;
                        hasResult = true;

                        HideCampaignDialog();

                    }

                    void Withdraw(CampaignDialogView view) {
                        HideCampaignDialog();

                    }

                    // Subscribe to events
                    engagementDialogView.SubscribeToDialogEvent(CampaignEngagementDialogView.WITHDRAW, Withdraw);
                    engagementDialogView.SubscribeToDialogEvent(CampaignEngagementDialogView.ENGAGE, Attack);

                    // Set formations etc.
                    engagementDialogView.SetAttackingFormations(engagementData.defendingFormations, this.Controller.Locale); // This is actually the defenders
                    engagementDialogView.SetupMatchData(this.Controller);

                    this.ShowCampaignDialog(engagementDialogView); 
                
                });

                // Wait until we've gotten a result
                while (this.CampaignDialogVisible == Visibility.Visible && !hasResult) {
                    Thread.Sleep(1);
                }

            }

            // Show battle status
            this.ShowBattleStatusDialog();

            // Return engagement data
            return engagementData;

        }
        
        public override void StateOnFocus() {
            
            // Start functionality
            if (!this.m_hasStarted) {

                // Invoke setup functionality and begin playing.
                this.Controller.StartCampaign();

                // Update flag
                this.m_hasStarted = true;

            }

        }

        public override void StateOnLostFocus() {}

        private void CreateNodeNetwork() {

            // Loop through all nodes
            this.Controller.Map.EachNode(n => {

                // TODO: Check if leaf then create different model
                ICampaignPointsNode visualNode = new CampaignVisualNodeModel(n, this.ResourceContext) {
                    ModelFromFormation = this.FromFormation
                };

                // Add click check
                visualNode.NodeClicked += this.NodeClicked;

                // Add node
                this.CampaignMapCanvas.Children.Add(visualNode.VisualElement);

                // Add node to list
                this.m_nodes.Add(visualNode);

            });

            // Loop through all transitions
            this.Controller.Map.EachTransition(t => {

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

        private void RefreshDisplayedFormations() {

            // Clear all formations
            this.ClearFormations();

            // Loop through all formations and display them
            this.Controller.Map.EachFormation(f => {

                // Banner file
                string banner_element = $"{f.Army}_banner";

                // Create space for UI element (default to null)
                UIElement displayElement = null;

                // If it exists, use an image, otherwise some other default pinkish shape
                if (this.m_resourceContext.GetResource(banner_element) is ImageSource img) {

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

                }

                // Create container type
                CampaignUnitFormationModel cufv = new CampaignUnitFormationModel(displayElement, f, this.ResourceContext) {
                    NodeModelFetcher = this.FromNode,
                    UnitModelFetcher = this.FromFormation,
                    ThreadDispatcher = this.ThreadDispatcher,
                    RemoveHandle = this.RemoveFormation
                };

                // Add to formation view
                this.m_formationViews.Add(cufv);

            });

            // Refresh formations
            this.m_nodes.ForEach(x => (x as CampaignVisualNodeModel)?.RefreshFormations(this.m_formationViews));

        }

        private void ClearFormations() {
            foreach (var form in this.m_formationViews) {
                if (form.VisualElement is not null) {
                    this.CampaignMapCanvas.Children.Remove(form.VisualElement);
                }
            }
            this.m_formationViews.Clear();
        }

        private void NodeClicked(ICampaignMapNode node, bool isRightclick) {
            if (isRightclick) {
                this.NodeRightClicked(node);
            } else {
                this.Selection.Select(this.FromNode(node), true);
                node.Occupants.ForEach(x => this.Selection.AddToSelection(this.FromFormation(x)));
            }
        }

        private void NodeRightClicked(ICampaignMapNode node) {
            if (this.FilterSelectionToSelf() > 0) {
                if (this.Selection.Shares(x => x.Formation.Node)) {
                    if (this.Selection.Filter(x => x.Formation.CanMove) > 0) {
                        // Get first unit and get path. If node is occupied by enemy, handle attack.
                        var first = this.Selection.First;
                        if (this.Controller.FindPath(first.Formation, node)) {
                            if (this.Selection.All(x => this.Controller.MoveFormation(x.Formation, node) == MoveFormationResult.MoveAttack)) {
                                this.Controller.HandleAttack(node, this.Selection.ToArray());
                            }
                        }

                    }
                }
            }
        }

        private void BannerClicked(object sender, MouseButtonEventArgs e) {
            if (sender is Image img && img.Tag is ICampaignFormation formation) {
                if (this.FromFormation(formation) is CampaignUnitFormationModel model) {
                    if (e.LeftButton == MouseButtonState.Pressed) {
                        Keyboard.IsKeyDown(Key.LeftShift).IfTrue().Then(() => this.Selection.AddToSelection(model)).Else(() => this.Selection.Select(model));
                    }
                }
            }
        }

        private ICampaignPointsNode FromNode(ICampaignMapNode node) => this.m_nodes.FirstOrDefault(x => x.Node == node);

        private CampaignUnitFormationModel FromFormation(ICampaignFormation formation) => this.m_formationViews.FirstOrDefault(x => x.Formation == formation);

        private void EndTurnBttn_Click(object sender, RoutedEventArgs e) {
            if (!this.Controller.EndTurn()) {
                this.Controller.EndCampaign();
            }
        }

        private int FilterSelectionToSelf() => this.Selection.Filter(x => x.Formation.Team == this.Controller.GetSelf().Team.Team);

        private void ShowBattleStatusDialog() {
            // TODO: Show battle status
        }

        private void SetupEngagementData(ref CampaignEngagementData engagementData, CampaignEngagementDialogView view) {

            // Setup base data
            engagementData.node = view.MapNode;
            engagementData.scenario = view.EngagementScenario;
            engagementData.allParticipatingSquads = new List<Squad>();
            engagementData.attackers = view.Attackers.ToLower() == "allies" ? CampaignArmyTeam.TEAM_ALLIES : CampaignArmyTeam.TEAM_AXIS;
            engagementData.defenders = engagementData.attackers == CampaignArmyTeam.TEAM_AXIS ? CampaignArmyTeam.TEAM_ALLIES : CampaignArmyTeam.TEAM_AXIS;
            engagementData.attackingFaction = view.AttackingRegimentalPool.First().Regiment.ElementOf.EleemntOf.Faction;
            engagementData.defendingFaction = view.MapNode.Occupants.First().Regiments.First().ElementOf.EleemntOf.Faction;
            engagementData.attackingFormations = this.Selection.Get();
            engagementData.defendingFormations = view.MapNode.Occupants;

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

        private void RemoveFormation(CampaignUnitFormationModel model) {
            if (this.m_formationViews.Remove(model)) {
                this.CampaignMapCanvas.Children.Remove(model.VisualElement);
            }
        }

    }

}
