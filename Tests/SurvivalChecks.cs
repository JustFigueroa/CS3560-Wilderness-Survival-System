using WildernessGameplay;
using System;
using System.Linq;
class SurvivalChecks {
 static Trader MakeTrader(TraderMood mood=TraderMood.Patient, bool offerPending=false, int offerAmount=0, int counterPrice=0,
  SupplyKind counterSupply=SupplyKind.Food, bool isCounterOffer=false) {
  var t=Trader.Create(mood); t.OfferPending=offerPending; t.OfferAmount=offerAmount; t.CounterPrice=counterPrice;
  t.CounterSupply=counterSupply; t.IsCounterOffer=isCounterOffer; return t;
 }

 static int checks;
 static void Check(bool x,string m){checks++;if(!x)throw new Exception(m);}
 static Game New(int width=6,int height=3) {
  var p=DifficultyPreset.Create(DifficultyLevel.Easy);
  p.boardWidth=width;p.boardHeight=height;
  p.terrainRules=new[]{new TerrainRule{terrain=TerrainKind.Plains,weight=1,strengthCost=1,foodCost=1,waterCost=1}};
  p.foodSpawnChance=p.waterSpawnChance=p.goldSpawnChance=p.traderSpawnChance=0;
  return WildernessGameplay.GameFactory.CreateSession(new GameSettings(p,42));
 }
 static string Fingerprint(Game g)=>string.Join(";",g.Tiles.Cast<Square>().Select(t=>$"{t.Terrain}/{t.Food}/{t.Water}/{t.Gold}/{t.Trader?.Mood}"));
 static void Main(){
  var g=New();var p=g.Player;
  Check(g.Phase==SessionPhase.Running&&p.X==0,"start");
  Check(g.Message == "", "starting action message is empty");
  Check(!g.Move(-1,p.Y)&&g.Turn==0,"invalid move consumed turn");
  int food=p.currentFood,water=p.currentWater,strength=p.currentStrength;
  Check(g.Move(1,p.Y)&&g.Turn==1&&p.currentFood==food-1&&p.currentWater==water-1&&p.currentStrength==strength-1,"move costs");
  var destination=g.Tiles[2,p.Y];destination.Food=10;destination.Water=10;destination.Gold=4;
  p.currentFood=p.maxFood-2;p.currentWater=p.maxWater-3;
  g.Move(2,p.Y);
  Check(g.Turn==2 && g.Current==destination,"entry collection uses one movement turn");
  Check(destination.Food==7&&destination.Water==6&&destination.Gold==0&&p.currentGold==g.Rules.startingGold+4,"entry collection conservation");
  Check(p.currentFood==p.maxFood&&p.currentWater==p.maxWater,"entry collection has no extra upkeep");
  int savedFood=destination.Food;
  Check(!g.Move(2,p.Y)&&destination.Food==savedFood&&g.Turn==2,"invalid move cannot collect twice");
  var rescue=New(2);rescue.Player.currentFood=1;rescue.Tiles[1,rescue.Player.Y].Food=3;
  rescue.Move(1,rescue.Player.Y);
  Check(rescue.Player.currentFood==3&&rescue.Phase==SessionPhase.Won,"pickup resolves before outcome");
  var brainMove=New();var startingTile=brainMove.Current;startingTile.Gold=5;
  brainMove.PlayTurn();
  Check(brainMove.Current!=startingTile&&brainMove.Turn==1&&startingTile.Gold==5,"brain does not spend a separate collection turn");
  int oldStrength=p.currentStrength;g.Rest();Check(p.currentStrength==Math.Min(p.maxStrength,oldStrength+g.Rules.restStrength),"rest");
  var lose=New();lose.Player.currentFood=1;lose.Move(1,lose.Player.Y);Check(lose.Phase==SessionPhase.Lost,"zero food defeat");
  var win=New(2);win.Move(1,win.Player.Y);Check(win.Phase==SessionPhase.Won,"east victory");
  int turn=win.Turn;Check(!win.Rest()&&!win.Move(0,win.Player.Y)&&win.Turn==turn,"terminal actions");
  var simultaneous=New(2);simultaneous.Player.currentWater=1;simultaneous.Move(1,simultaneous.Player.Y);
  Check(simultaneous.Phase==SessionPhase.Lost,"defeat priority");
  // Entering a trader tile always presents an offer, even with full supplies or no gold.
  foreach (TraderMood mood in Enum.GetValues(typeof(TraderMood))) {
   var encounter=New(); encounter.Player.currentFood=encounter.Player.maxFood; encounter.Player.currentWater=encounter.Player.maxWater;
   encounter.Player.currentGold=0; var tile=encounter.Tiles[1,encounter.Player.Y]; tile.Trader=MakeTrader(mood:mood);
   encounter.Move(1,encounter.Player.Y);
   Check(tile.Trader.NextAction==TraderAction.Offer && !encounter.Message.Contains("Trader"), "entry schedules offer separately");
   Check(encounter.AdvanceTrader() && tile.Trader.OfferPending && encounter.Message.StartsWith(mood + " Trader provides offer:") && !encounter.Message.Contains("Used:"), "next presents trader offer only");
   int before=encounter.Turn; encounter.PlayTurn();
   Check(encounter.Turn==before+1 && !tile.Trader.OfferPending && encounter.Player.X==1 && encounter.Message.Contains("Player rejects offer."), "brain visibly rejects unusable offer without moving");
   int foodBeforeLeave=encounter.Player.currentFood;
   Check(encounter.PlayTurn() && encounter.Message=="Player leaves." && encounter.Player.X==1 && encounter.Player.currentFood==foodBeforeLeave, "next leaves without upkeep");
  }
  var generous=New();generous.Player.currentFood=3;generous.Player.currentWater=20;
  generous.Tiles[1,generous.Player.Y].Trader=MakeTrader(mood:TraderMood.Generous);generous.Move(1,generous.Player.Y);
  generous.AdvanceTrader();
  Check(generous.Current.Trader.CounterPrice==1 && generous.Current.Trader.OfferAmount==6,"generous double bundle for one gold");
  int goldBefore=generous.Player.currentGold, foodBefore=generous.Player.currentFood, tradeTurn=generous.Turn;
  generous.PlayTurn();
  Check(generous.Player.currentGold==goldBefore-1 && generous.Player.currentFood==foodBefore+5 && generous.Turn==tradeTurn+1,"brain accepts generous offer with exact exchange and upkeep");
  Check(generous.Message.Contains("Used: -0 str        -1 food        -1 water        -1 gold") && generous.Message.Contains("Gained: +0 str        +6 food"),"trade gross accounting");
  Check(!generous.Trade(SupplyKind.Food,true),"completed offer cannot be accepted twice");
  foreach(TraderMood mood in new[]{TraderMood.Patient,TraderMood.Generous,TraderMood.Impatient}) {
   var negotiation=New();negotiation.Player.currentFood=20;negotiation.Player.currentWater=20;negotiation.Player.currentGold=0;
   negotiation.Current.Trader=MakeTrader(mood:mood,offerPending:true,offerAmount:3,counterPrice:6,counterSupply:SupplyKind.Food);
   for(int n=0;n<5;n++) {
    if(!negotiation.Current.Trader.OfferPending)break;
    Check(negotiation.Trade(SupplyKind.Food,false),"counteroffer takes an action");
    Check(negotiation.Message.StartsWith("Player provides counteroffer:") && !negotiation.Message.Contains("Trader") && !negotiation.Message.Contains("Used:"), "player counteroffer has its own message");
    int foodAfterPlayer=negotiation.Player.currentFood;
    Check(foodAfterPlayer==20 && negotiation.Player.currentWater==20,"counteroffers do not charge upkeep");
    Check(negotiation.AdvanceTrader() && negotiation.Message.StartsWith(mood + " Trader") && !negotiation.Message.Contains("Player") && negotiation.Player.currentFood==foodAfterPlayer, "trader response is separate and has no extra upkeep");
   }
   Check(mood==TraderMood.Impatient ? !negotiation.Current.Trader.OfferPending && negotiation.Current.Trader.Negotiations==2 : negotiation.Current.Trader.OfferPending && negotiation.Current.Trader.Negotiations==5,"trader-specific counteroffer persistence");
  }
  var expensive=New(); expensive.Player.currentFood=20; expensive.Player.currentWater=20; expensive.Player.currentGold=1;
  expensive.Current.Trader=MakeTrader(mood:TraderMood.Patient,offerPending:true,offerAmount:3,counterPrice:6,counterSupply:SupplyKind.Food);
  for(int n=0;n<3;n++) {
   Check(expensive.PlayTurn() && expensive.Message.StartsWith("Player provides counteroffer: 1 gold"), "brain keeps countering unaffordable offers");
   Check(expensive.PlayTurn() && expensive.Message.StartsWith("Patient Trader provides counteroffer:"), "patient responds on separate next");
  }
  Check(expensive.DeclineTrade() && expensive.Message=="Player rejects counteroffer.", "counteroffer rejection label");
  Check(expensive.PlayTurn() && expensive.Message=="Player leaves." && !expensive.Current.Trader.OfferPending, "rejected counteroffer leaves next");
  var completedCounter=New(); completedCounter.Player.currentFood=10; completedCounter.Player.currentWater=20;
  completedCounter.Current.Trader=MakeTrader(offerPending:true,offerAmount:3,counterPrice:6);
  completedCounter.Trade(SupplyKind.Food,false);
  Check(completedCounter.Player.currentFood==10 && completedCounter.Player.currentWater==20,"counteroffer is free");
  completedCounter.AdvanceTrader();
  Check(completedCounter.Player.currentFood==12 && completedCounter.Player.currentWater==19 && !completedCounter.Current.Trader.OfferPending,"accepted counteroffer charges one upkeep");
  var mediumRules=DifficultyPreset.Create(DifficultyLevel.Medium);
  var mediumResources=GameFactory.CreateSession(new GameSettings(mediumRules,42));
  Check(mediumRules.restStrength==5 && mediumRules.minimumGoldAmount==3 && mediumRules.maximumGoldAmount==6,"medium recovery and gold range");
  Check(mediumResources.Tiles.Cast<Square>().All(t=>(t.Food==0 || (t.Food>=2 && t.Food<=4)) && (t.Water==0 || (t.Water>=2 && t.Water<=4)) && (t.Gold==0 || (t.Gold>=3 && t.Gold<=6))),"medium food/water and gold ranges are independent");
  var badGold=mediumRules.Copy();badGold.minimumGoldAmount=7;
  Check(!badGold.TryValidate(out _),"invalid gold range is rejected");
  int changedInitial=0,changedCounter=0;
  for(int seed=0;seed<200;seed++) {
   foreach(bool isCounter in new[]{false,true}) {
    var rules=DifficultyPreset.Create(DifficultyLevel.Easy);
    var uncertain=GameFactory.CreateSession(new GameSettings(rules,seed));uncertain.Player.currentFood=10;uncertain.Player.currentWater=20;uncertain.Player.currentGold=20;
    uncertain.Current.Trader=MakeTrader(mood:TraderMood.Indecisive,offerPending:true,offerAmount:3,counterPrice:4,counterSupply:SupplyKind.Food,isCounterOffer:isCounter);
    uncertain.Trade(SupplyKind.Food,true);
    Check(uncertain.Message.StartsWith("Player accepts") && !uncertain.Message.Contains("Trader") && !uncertain.Message.Contains("Used:"), "indecisive player acceptance waits for trader");
    uncertain.AdvanceTrader();
    if(uncertain.Current.Trader.OfferPending) {
     Check(uncertain.Player.currentFood==10 && uncertain.Player.currentWater==20,"indecisive revision has no upkeep");
     if(isCounter)changedCounter++;else changedInitial++;
     Check(uncertain.Player.currentGold==20 && uncertain.Message.StartsWith("Indecisive trader becomes indecisive. Indecisive Trader provides counteroffer:"),"revised indecisive offer does not exchange resources");
    } else Check(uncertain.Player.currentGold==16 && uncertain.Player.currentFood==12 && uncertain.Player.currentWater==19 && uncertain.Message.Contains("Used: -0 str        -1 food        -1 water        -4 gold"),"settled indecisive trade charges upkeep exactly once");
   }
  }
  Check(changedInitial>65&&changedInitial<135&&changedCounter>65&&changedCounter<135,"indecisive initial and counter offers use a roughly fifty percent seeded roll");
  foreach(DifficultyLevel level in Enum.GetValues(typeof(DifficultyLevel))) {
   var distribution=GameFactory.CreateSession(new GameSettings(DifficultyPreset.Create(level),42));
   foreach(var group in distribution.Tiles.Cast<Square>().GroupBy(t=>t.Terrain))
    Check(group.Select(t=>$"{t.Food}/{t.Water}/{t.Gold}").Distinct().Count()>1,"same terrain has independently varied resources");
  }
  var known=New(10,10);Check(!known.Explored[9,9],"fog");
  foreach(BrainMode mode in Enum.GetValues(typeof(BrainMode))) {
   var local=New();local.Rules.brain=mode;local.Player.X=2;local.Player.Y=1;
   local.Player.currentFood=10;local.Player.currentWater=20;local.RefreshVision();
   local.Tiles[2,2].Food=6;
   Check(local.PlayTurn(), "neighbor brain takes action");
   Check(mode==BrainMode.ResourceAware ? local.Player.X==2&&local.Player.Y==2 : local.Player.X==3&&local.Player.Y==1,
    "ResourceAware chooses needed adjacent supply; EastFirst chooses east");
  }
  foreach(BrainMode mode in Enum.GetValues(typeof(BrainMode))) {
   var urgent=New();urgent.Rules.brain=mode;urgent.Player.X=2;urgent.Player.Y=1;
   urgent.Player.currentFood=5;urgent.Player.currentWater=20;urgent.Tiles[2,2].Food=6;urgent.RefreshVision();
   Check(urgent.PlayTurn()&&urgent.Player.X==2&&urgent.Player.Y==2,"both brains prioritize critical food over east");
   var scarce=New();scarce.Rules.brain=mode;scarce.Player.X=2;scarce.Player.Y=1;
   scarce.Player.currentFood=8;scarce.Player.currentWater=3;
   scarce.Tiles[3,1].Food=6;scarce.Tiles[2,2].Water=6;scarce.RefreshVision();
   Check(scarce.PlayTurn()&&scarce.Player.X==2&&scarce.Player.Y==2,"both brains seek water before less urgent food");
   var exchangeShortage=New();exchangeShortage.Rules.brain=mode;exchangeShortage.Player.X=2;exchangeShortage.Player.Y=1;
   exchangeShortage.Player.currentFood=3;exchangeShortage.Player.currentWater=4;
   exchangeShortage.Tiles[3,1].Food=6;exchangeShortage.Tiles[3,1].WaterCost=3;
   exchangeShortage.Tiles[2,2].Food=4;exchangeShortage.RefreshVision();
   Check(exchangeShortage.PlayTurn()&&exchangeShortage.Player.X==2&&exchangeShortage.Player.Y==2,"supply detour does not create a worse water shortage");
  }
  var hidden=New();hidden.Tiles[1,hidden.Player.Y].Food=6;
  System.Array.Clear(hidden.Visible,0,hidden.Visible.Length);
  Check(!hidden.PlayTurn()&&hidden.Turn==0&&hidden.Phase==SessionPhase.Lost,"neighbor brain does not inspect hidden tiles");
  var revisit=New();revisit.Player.Y=1;revisit.RefreshVision();revisit.Visits[1,1]=1;
  Check(revisit.PlayTurn()&&revisit.Player.X==1&&revisit.Player.Y==2,"unvisited northeast precedes visited east");
  var restBrain=New();restBrain.Player.currentStrength=0;
  Check(restBrain.PlayTurn()&&restBrain.Player.X==0&&restBrain.Turn==1&&restBrain.Message=="Rested.","brain rests for an adjacent move");
  var preset=DifficultyPreset.Create(DifficultyLevel.Medium);
  int originalWeight=preset.terrainRules[0].weight;
  var settings=new GameSettings(preset,11);settings.difficultyValues.terrainRules[0].weight=99;
  Check(preset.terrainRules[0].weight==originalWeight,"deep copy");
  var a=GameFactory.CreateSession(new GameSettings(preset,55));var b=GameFactory.CreateSession(new GameSettings(preset,55));
  Check(Fingerprint(a)==Fingerprint(b)&&a.Player.Y==b.Player.Y,"seed reproducibility");
  var zero=DifficultyPreset.Create(DifficultyLevel.Easy);zero.foodSpawnChance=zero.waterSpawnChance=zero.goldSpawnChance=zero.traderSpawnChance=0;
  Check(GameFactory.CreateSession(new GameSettings(zero,1)).Tiles.Cast<Square>().All(t=>t.Food==0&&t.Water==0&&t.Gold==0&&t.Trader==null),"zero probabilities");
  var regenRules=DifficultyPreset.Create(DifficultyLevel.Easy);regenRules.repeatingResources=true;regenRules.resourceRespawnTurns=2;regenRules.foodSpawnChance=1;
  var regen=GameFactory.CreateSession(new GameSettings(regenRules,2));regen.Player.currentFood=10;
  var regenTile=regen.Tiles[1,regen.Player.Y];int initial=regenTile.Food;
  regen.Move(1,regen.Player.Y);
  Check(regenTile.Food==0,"entry depletes resources that fit inventory");
  regen.Player.currentStrength=0;regen.Rest();regen.Player.currentStrength=0;regen.Rest();
  Check(regenTile.Food==initial,"resource renewal");
  var limitRules=DifficultyPreset.Create(DifficultyLevel.Easy);limitRules.turnLimit=1;
  var limit=GameFactory.CreateSession(new GameSettings(limitRules,3));limit.Rest();Check(limit.Phase==SessionPhase.Lost,"turn limit");
  var mountain=New();mountain.Tiles[1,mountain.Player.Y].StrengthCost=5;
  mountain.Player.currentStrength=4;
  Check(!mountain.Move(1,mountain.Player.Y)&&mountain.Turn==0,"mountain blocks insufficient strength");
  int foodBeforeRest=mountain.Player.currentFood;
  Check(mountain.Rest()&&mountain.Player.currentStrength==4+mountain.Rules.restStrength&&mountain.Player.currentFood==foodBeforeRest-1,"rest funds mountain movement with upkeep");
  Check(mountain.Move(1,mountain.Player.Y)&&mountain.Player.currentStrength==4+mountain.Rules.restStrength-5&&mountain.Turn==2,"rest then mountain move");
   foreach (TerrainKind terrain in Enum.GetValues(typeof(TerrainKind))) {
    var visionRules=DifficultyPreset.Create(DifficultyLevel.Easy);
    visionRules.boardWidth=visionRules.boardHeight=15;
    visionRules.terrainRules=new[]{new TerrainRule{terrain=terrain,weight=1,strengthCost=1,foodCost=1,waterCost=1}};
    var visionGame=GameFactory.CreateSession(new GameSettings(visionRules,42));
    Check(visionGame.CurrentVision==(terrain==TerrainKind.Forest?VisionMode.Cautious:terrain==TerrainKind.Swamp?VisionMode.KeenEyed:terrain==TerrainKind.Mountain?VisionMode.Focused:VisionMode.FarSight), "spawn vision follows spawn terrain");
    visionGame.Player.X=6;visionGame.Player.Y=7;
    visionGame.Current.Terrain=TerrainKind.Mountain;
    visionGame.RefreshVision();
    Check(visionGame.CurrentVision==VisionMode.Focused,"current terrain determines vision on refresh");
    var entered=visionGame.Tiles[7,7];entered.Terrain=terrain;
    entered.StrengthCost=entered.FoodCost=entered.WaterCost=1;
    Check(visionGame.Move(7,7),"move into vision terrain");
    var expected=terrain==TerrainKind.Forest?VisionMode.Cautious:
     terrain==TerrainKind.Swamp?VisionMode.KeenEyed:
     terrain==TerrainKind.Mountain?VisionMode.Focused:VisionMode.FarSight;
    Check(visionGame.CurrentVision==expected,"terrain selects vision");
    int visibleCount=visionGame.Visible.Cast<bool>().Count(value=>value);
    Check(visibleCount==(expected==VisionMode.FarSight?41:expected==VisionMode.KeenEyed?25:13),"terrain vision shape and range");
    Check(visionGame.Rest()&&visionGame.CurrentVision==expected,"rest preserves terrain vision");
    visionGame.Tiles[6,7].Terrain=TerrainKind.Plains;
    visionGame.Tiles[6,7].StrengthCost=visionGame.Tiles[6,7].FoodCost=visionGame.Tiles[6,7].WaterCost=1;
    Check(visionGame.Move(6,7)&&visionGame.CurrentVision==VisionMode.FarSight,"plains replaces previous vision");
   }
  var events = new System.Collections.Generic.List<string>();
  var logged = New(); logged.EventReported += events.Add;
  logged.Move(1, logged.Player.Y);
  Check(events[0] == "Entered a Plains tile." && logged.Message == string.Join("\n", events), "movement console events match HUD summary");
  Check(logged.Message.Contains("Used: -1 str        -1 food        -1 water        -0 gold"), "widely spaced movement costs");
  var pickup = New(); pickup.Tiles[1,pickup.Player.Y].Gold=5; pickup.Move(1,pickup.Player.Y);
  Check(pickup.Player.currentGold==pickup.Rules.startingGold+5 && pickup.Message.Contains("Gained: +0 str        +0 food        +0 water        +5 gold"), "pickup gains are reported");
  events.Clear(); logged.Player.currentStrength = logged.Player.maxStrength - 1; logged.Rest();
  Check(events.Count == 1 && events[0] == "Rested." && !logged.Message.Contains("Entered"), "rest replaces previous action summary");
  Check(logged.Player.currentStrength==logged.Player.maxStrength && !logged.Message.Contains("Used:") && !logged.Message.Contains("Gained:"), "rest still recovers strength without resource lines");
  var loggedWin = New(2); events.Clear(); loggedWin.EventReported += events.Add; loggedWin.Move(1, loggedWin.Player.Y);
  Check(loggedWin.Phase==SessionPhase.Won && loggedWin.Message.Contains("Entered") && !loggedWin.Message.Contains("You win") && events.Count==3, "victory keeps three action lines and uses the popup for its result");
  var blocked = New(); events.Clear(); blocked.EventReported += events.Add;
  foreach(var tile in blocked.Tiles) tile.StrengthCost = blocked.Player.maxStrength + 1;
  int blockedFood=blocked.Player.currentFood, blockedWater=blocked.Player.currentWater, blockedStrength=blocked.Player.currentStrength, blockedGold=blocked.Player.currentGold;
  Check(!blocked.PlayTurn() && blocked.Phase==SessionPhase.Lost, "no available action ends the game");
  Check(blocked.Turn==0 && blocked.Player.currentFood==blockedFood && blocked.Player.currentWater==blockedWater && blocked.Player.currentStrength==blockedStrength && blocked.Player.currentGold==blockedGold, "failed search does not consume turn or resources");
  Check(events.Count==0 && !blocked.Message.Contains("No action"), "stranded defeat uses popup only");
  Check(!blocked.PlayTurn() && blocked.Turn==0, "retry with no action remains safe");
  blocked.Tiles[1,blocked.Player.Y].StrengthCost=1;
  Check(!blocked.PlayTurn() && blocked.Turn==0 && blocked.Phase==SessionPhase.Lost && !blocked.Move(1,blocked.Player.Y) && !blocked.Rest(), "stranded defeat prevents subsequent actions");
  Check(!lose.Message.Contains("You lose") && !limit.Message.Contains("You lose"), "resource and turn-limit defeats use popup only");
  // Visible path search: closest is distance, easiest is strength among viable candidates.
  var routing=New(9,9); routing.Player.X=4; routing.Player.Y=4; routing.RefreshVision();
  var near=routing.Tiles[5,4]; var far=routing.Tiles[4,6];
  near.Food=near.Water=near.Gold=6; near.StrengthCost=5; near.Trader=new Patient();
  far.Food=far.Water=far.Gold=6; far.Trader=new Patient();
  int routeFood=routing.Player.currentFood, routeTurn=routing.Turn;
  var routes=new VisiblePaths(routing);
  Check(routes.ClosestFood.Destination==near && routes.SecondClosestFood.Destination==far,"food targets ordered by moves");
  Check(routes.ClosestWater.Destination==near && routes.SecondClosestWater.Destination==far &&
   routes.ClosestGold.Destination==near && routes.SecondClosestGold.Destination==far &&
   routes.ClosestTrader.Destination==near && routes.SecondClosestTrader.Destination==far,"all four target pairs are distinct and ordered");
  Check(routes.ClosestFood.Steps.Count==1 && routes.SecondClosestFood.Steps.Count==2,"BFS shortest step counts");
  Check(routes.SecondClosestFood.MovementCost==2 && routes.SecondClosestFood.FoodCost==2 && routes.SecondClosestFood.WaterCost==2,"route excludes start and totals entry costs");
  Check(VisiblePaths.Easiest(new[]{routes.ClosestFood,routes.SecondClosestFood})==routes.SecondClosestFood,"second nearest can be easier");
  Check(routing.Player.currentFood==routeFood && routing.Turn==routeTurn && near.Food==6 && far.Food==6,"planning does not mutate the game");
  routing.Visible[far.X,far.Y]=false;
  Check(new VisiblePaths(routing).SecondClosestFood==null,"hidden resource excluded despite explored memory");
  Array.Clear(routing.Visible,0,routing.Visible.Length); routing.Visible[far.X,far.Y]=true;
  Check(new VisiblePaths(routing).ClosestFood==null,"cannot route through hidden tiles to visible target");

  var survivalPath=New(6,1); survivalPath.Player.currentFood=2; survivalPath.Player.currentWater=10; survivalPath.Player.currentStrength=0;
  survivalPath.Tiles[1,0].Food=4;
  var safePath=new VisiblePaths(survivalPath).All.First(path=>path.Destination==survivalPath.Tiles[1,0]);
  Check(safePath.Survivable && safePath.FoodAfter==4,"rest plus entry pickup can rescue food after exact payment");
  Check(survivalPath.Player.currentFood==2 && survivalPath.Player.currentStrength==0,"rest simulation does not mutate player");
  survivalPath.Player.currentFood=1;
  Check(!new VisiblePaths(survivalPath).All.First(path=>path.Destination==survivalPath.Tiles[1,0]).Survivable,"rest cannot kill player before pickup");
  survivalPath.Player.currentStrength=10; survivalPath.Player.currentFood=2; survivalPath.Tiles[1,0].Food=0; survivalPath.Tiles[3,0].Food=6;
  var deadly=new VisiblePaths(survivalPath);
  Check(!deadly.ClosestFood.Survivable && VisiblePaths.Easiest(new[]{deadly.ClosestFood})==null,"later food cannot rescue death on intermediate tile");
  survivalPath.Tiles[1,0].Food=100;
  var capped=new VisiblePaths(survivalPath).All.First(path=>path.Destination==survivalPath.Tiles[1,0]);
  Check(capped.FoodAfter==survivalPath.Player.maxFood,"simulated pickup respects capacity");
  survivalPath.Rules.turnLimit=1; survivalPath.Player.currentStrength=0;
  Check(!new VisiblePaths(survivalPath).All.First(path=>path.Destination==survivalPath.Tiles[1,0]).Survivable,"route rest respects turn limit");

  foreach(BrainMode mode in Enum.GetValues(typeof(BrainMode))) {
   var distant=New(9,9); distant.Rules.brain=mode; distant.Player.X=4; distant.Player.Y=4;
   distant.Player.currentFood=10; distant.Player.currentWater=20; distant.Tiles[4,6].Food=6; distant.RefreshVision();
   Check(distant.PlayTurn() && distant.Turn==1,"route brain takes only one step");
   Check(mode==BrainMode.ResourceAware ? distant.Player.Y==5 : distant.Player.X==5 && distant.Player.Y==4,
    "ResourceAware heads toward distant supplies while EastFirst advances east");
  }
  var eastFallback=New(9,9); eastFallback.Rules.brain=BrainMode.EastFirst;
  eastFallback.Player.X=4; eastFallback.Player.Y=4; eastFallback.Player.currentFood=5;
  for(int x=5;x<9;x++)for(int y=0;y<9;y++)eastFallback.Tiles[x,y].StrengthCost=21;
  eastFallback.Tiles[4,6].Food=6; eastFallback.RefreshVision();
  Check(eastFallback.PlayTurn() && eastFallback.Player.Y==5,"EastFirst uses resource paths when east is blocked");
  var alternate=New(9,9); alternate.Player.X=4; alternate.Player.Y=4; alternate.Player.currentFood=5;
  alternate.Tiles[5,4].Food=6; alternate.Tiles[5,4].FoodCost=10;
  alternate.Tiles[4,6].Food=6; alternate.RefreshVision();
  Check(alternate.PlayTurn() && alternate.Player.Y==5,"brain uses second target when closest is unsafe");
  // Verify that the diagram classes, not unused scaffolds, own the running model.
  foreach(DifficultyLevel level in Enum.GetValues(typeof(DifficultyLevel))) {
   var model=GameFactory.CreateSession(new GameSettings(DifficultyPreset.Create(level),9));
   Check(model.Difficulty.GetType().Name==level.ToString(), "factory selects concrete difficulty");
   Check(ReferenceEquals(model.Tiles,model.Map.Tiles) && ReferenceEquals(model.Player.OccupiedSquare,model.Current), "one authoritative map and occupied square");
   Check(model.Player.Brain is ResourceAware && model.Player.Vision.Mode==model.CurrentVision,"factory initializes concrete strategies");
   foreach(var square in model.Tiles) {
    Check(square.TerrainType.GetType().Name==square.Terrain.ToString(),"terrain uses its concrete class");
    Check(square.Items.Contains(square.FoodBonus) && square.Items.Contains(square.WaterBonus) && square.Items.Contains(square.GoldBonus),"square stores bonus items");
    if(square.Trader!=null) Check(square.Items.Contains(square.Trader) && square.Trader.GetType().Name==square.Trader.Mood.ToString(),"square stores concrete trader item");
   }
   var modelPaths=model.Player.Vision.FindPaths(model);
   Check(ReferenceEquals(model.Player.Vision.ClosestFood(), modelPaths.ClosestFood) && ReferenceEquals(model.Player.Vision.SecondClosestTrader(),modelPaths.SecondClosestTrader),"vision exposes shared path search");
   foreach(var route in modelPaths.All) Check(route.Moves.Count==route.Steps.Count,"path includes one direction per step");
  }
  var decisionGame=New(); decisionGame.Player.currentStrength=0;
  Check(decisionGame.Player.Brain.MakeMove(decisionGame).Action==BrainAction.Rest,"brain returns explicit rest decision");
  decisionGame.Player.currentStrength=10;
  Check(decisionGame.Player.Brain.MakeMove(decisionGame).Action==BrainAction.Move,"brain returns explicit move decision");
  Array.Clear(decisionGame.Visible,0,decisionGame.Visible.Length);
  Check(decisionGame.Player.Brain.MakeMove(decisionGame).Action==BrainAction.NoAction,"brain distinguishes no action from rest");
  Console.WriteLine(checks+" rule checks passed.");
  foreach(DifficultyLevel level in Enum.GetValues(typeof(DifficultyLevel))) {
   int wins=0,losses=0,totalTurns=0;
   for(int seed=0;seed<30;seed++) {
    var sim=GameFactory.CreateSession(new GameSettings(DifficultyPreset.Create(level),seed));
    int guard=0;
    while(sim.Phase==SessionPhase.Running&&guard++<sim.Rules.turnLimit+1){
     int prior=sim.Turn;
     if (!sim.PlayTurn()) { Check(sim.Phase!=SessionPhase.Running,"no-action run is terminal"); break; }
     Check(sim.Turn>prior||sim.Phase!=SessionPhase.Running,"autoplay stalled");
     Check(sim.Player.currentFood>=0&&sim.Player.currentWater>=0&&sim.Player.currentStrength>=0&&sim.Player.currentGold>=0,"negative resources");
     Check(sim.Player.currentFood<=sim.Player.maxFood&&sim.Player.currentWater<=sim.Player.maxWater&&sim.Player.currentStrength<=sim.Player.maxStrength,"resource overflow");
    }
    Check(sim.Phase!=SessionPhase.Running,"nonterminating run");
    if(sim.Phase==SessionPhase.Won)wins++;
    else if(sim.Phase==SessionPhase.Lost)losses++;
    totalTurns+=sim.Turn;
   }
   Console.WriteLine(level+": "+wins+"/30 wins; "+losses+" losses; average turns "+totalTurns/30);
  }

  ulong trace = 14695981039346656037UL;
  int runs = 0;
  foreach (DifficultyLevel level in Enum.GetValues(typeof(DifficultyLevel)))
  foreach (BrainMode brain in Enum.GetValues(typeof(BrainMode)))
  for (int seed = 0; seed < 20; seed++) {
   var rules = DifficultyPreset.Create(level); rules.brain = brain;
   var sim = GameFactory.CreateSession(new GameSettings(rules, seed));
   while (sim.Phase == SessionPhase.Running) {
    int prior = sim.Turn;
    if (!sim.PlayTurn()) {
     Check(sim.Phase==SessionPhase.Lost, "no-action combination ends in defeat");
     break;
    }
    Check(sim.Turn > prior || sim.Phase != SessionPhase.Running, "combination stalled");
    var player = sim.Player;
    foreach (int value in new[]{sim.Turn, player.X, player.Y, player.currentStrength, player.currentFood, player.currentWater, player.currentGold, (int)sim.Phase})
     trace = unchecked((trace ^ (uint)value) * 1099511628211UL);
   }
   runs++;
  }
  Console.WriteLine("Behavior trace: " + trace + " across " + runs + " runs");
  Console.WriteLine("All "+checks+" checks passed.");
 }
}
