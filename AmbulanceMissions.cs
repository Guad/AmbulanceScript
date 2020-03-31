using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

public class GtaAmbulance : Script
{
    
    public GtaAmbulance()
    {
        KeyDown += OnKeyDown;
        Tick += OnTick;
        Interval = 1;
        _bigmessage = new UIText("PARAMEDIC", new Point(630, 100), 2.8f, Color.Goldenrod, GTA.Font.Monospace, true);
    }
    private const int PayoutMultiplier = 100;

    private UIText _headsup;
    private UIRectangle _headsupRectangle;
    private UIText _bigmessage;
    private Boolean _onAmbulanceMission;
    private Ped _victimPed;
    private Ped _oldVictim;
    private Blip _blip;
    private Random _rnd = new Random();

    private int _secondTicks;
    private int _ticks;
    private int _level = 1;
    private int _timer = 1000;
    private bool _showMessage;
    private int[] _difficultyScale = {100, 90, 85, 80, 75, 70, 60, 55, 50, 45, 40, 35, 30};
    private int _baseDamage = 900;


    private bool _critical;
    private bool _cprBeingApplied;
    private bool _cprAppliedThisMission;
    private bool _reanimationFailed;

    private int _missionState; // 0 - not started, 1 - going to patient, 2 - going to hospital, 3 - in hospital
    private bool _airMission;
    //private GTA.Math.Vector3 hospital = new GTA.Math.Vector3(1841.359f, 3668.857f, 33.679f);
    private readonly Vector3[] _hospitals = {
        new Vector3(-454.8831f, -340.2435f, 34.36343f),
        new Vector3(1842.389f, 3667.902f, 33.67993f),
        new Vector3(300.457f, -1441.283f, 29.79361f),
        new Vector3(-238.5425f, 6334.082f, 32.41678f),
        new Vector3(364.4797f, -592.1211f, 28.68582f)
    };

    private readonly Vector3[] _hospitalEntrances = {
        new Vector3(-448.5311f, -347.6872f, 34.50179f),
        new Vector3(294.4863f, -1448.287f, 29.9666f),
        new Vector3(359.7662f, -584.9894f, 28.81874f),
        new Vector3(1839.238f, 3673.694f, 34.27666f),
        new Vector3(-248.1741f, 6330.686f, 32.4262f)
    };

    private readonly Vector3[] _airHospitals = {
        new Vector3(313.6914f, -1464.372f, 46.89649f),
        new Vector3(352.8871f, -588.515f, 74.55135f)
    };

    private readonly Vector3[] _airHospitalEntrances = {
        new Vector3(391.1208f, -1432.876f, 29.43557f),
        new Vector3(339.185f, -584.1078f, 74.16563f)
    };

    private readonly Vector3[] _victims = {
        new Vector3(-155.1335f, 6264.229f, 31.48946f), //PALETO BAY
        new Vector3(-268.0224f, 6180.009f, 31.39273f),
        new Vector3(-283.2744f, 6223.528f, 31.4958f),
        new Vector3(-78.92438f, 6489.741f, 31.00436f),
        new Vector3(-52.61729f, 6526.674f, 31.02834f),
        new Vector3(175.5626f, 7040.976f, 1.468205f),
        new Vector3(-91.19389f, 6337.676f, 31.03658f), //Sandy Shores
        new Vector3(-100.5584f, 6411.113f, 31.02189f),
        new Vector3(-78.92438f, 6489.741f, 31.00436f),
        new Vector3(-52.61729f, 6526.674f, 31.02834f),
        new Vector3(54.18509f, 7085.758f, 2.171395f),
        new Vector3(175.5626f, 7040.976f, 1.468205f),
        new Vector3(-582.409f, 5315.361f, 69.99908f),
        new Vector3(-1633.15f, 4737.142f, 53.29347f),
        new Vector3(-124.679f, 4273.219f, 45.12233f),
        new Vector3(2094.326f, 4769.768f, 40.95844f),
        new Vector3(1963.848f, 5168.533f, 47.40821f),      
        new Vector3(2633.842f, 4527.676f, 36.76321f),
        new Vector3(1848.775f, 3924.496f, 32.77434f),
        new Vector3(1823.735f, 3766.912f, 33.16585f),
        new Vector3(1706.295f, 3749.585f, 33.75248f),
        new Vector3(1351.814f, 3593.798f, 34.63599f),
        new Vector3(1993.938f, 3060.152f, 46.81282f),
        new Vector3(2567.84f, 384.4785f, 108.2305f), 
        new Vector3(-1032.054f, -2725.19f, 13.16502f), //Los Santos
		new Vector3(-682.0075f, -2233.63f, 5.491331f),
		new Vector3(-396.4651f, -2270.628f, 7.144282f),
		new Vector3(59.72925f, -2562.665f, 5.499238f),
		new Vector3(229.4973f, -3113.489f, 5.328383f),
		new Vector3(824.1693f, -2981.625f, 5.618261f),
		new Vector3(806.4105f, -2131.054f, 28.87451f),
		new Vector3(358.6248f, -1980.638f, 23.81694f),
		new Vector3(88.3496f, -1508.658f, 28.87959f),
		new Vector3(471.5156f, -1519.8f, 28.83399f),
		new Vector3(714.605f, -1077.308f, 21.82764f),
		new Vector3(227.6832f, -957.8099f, 28.8784f),
		new Vector3(151.8911f, -1033.908f, 28.88036f),
		new Vector3(-57.86221f, -789.1469f, 43.77345f),
		new Vector3(258.0165f, -635.2186f, 40.29241f),
		new Vector3(247.5053f, -380.2834f, 44.06373f),
		new Vector3(165.3261f, -114.6701f, 61.74897f),
		new Vector3(237.3834f, -34.43047f, 69.25822f),
		new Vector3(546.7581f, -206.9675f, 53.62393f),
		new Vector3(632.5165f, 99.35558f, 88.50195f),
		new Vector3(883.628f, -133.382f, 77.66323f),
		new Vector3(1050.944f, -490.6486f, 63.45192f),
		new Vector3(1126.734f, -655.963f, 56.29839f),
		new Vector3(452.8423f, 126.7028f, 98.89735f),
		new Vector3(229.1809f, 341.4336f, 105.08f),
		new Vector3(-102.4168f, -120.8662f, 57.2532f),
		new Vector3(-356.6556f, 33.16491f, 47.33154f),
		new Vector3(-676.6347f, 299.3956f, 81.59211f),
		new Vector3(-891.4418f, -2.590907f, 42.91672f),
		new Vector3(-1419.533f, -197.3691f, 46.74275f),
		new Vector3(-1862.799f, -353.8355f, 48.7586f),
		new Vector3(-1887.052f, -570.1543f, 11.27715f),
		new Vector3(-1852.131f, -1223.636f, 12.54064f),
		new Vector3(-1279.246f, -1203.424f, 4.349295f),
		new Vector3(-708.0892f, -1397.278f, 4.671808f),
		new Vector3(-532.0626f, -1218.669f, 17.96088f),
		new Vector3(-603.4581f, -947.6642f, 21.64981f),
		new Vector3(-1092.189f, 433.6045f, 74.99454f),
		new Vector3(-747.6935f, 815.9718f, 212.9589f)
    };

    void OnTick(object sender, EventArgs e)
    {
        if (_ticks >= _timer)
        {
            _ticks = 0;
            _showMessage = false;
        }
        if (_showMessage)
        {
            _bigmessage.Draw();
            _ticks += 1;
        }
        Ped player = Game.Player.Character;


        if (_victimPed != null && _onAmbulanceMission)
        {
            if (_victimPed.Health <= 20 && !_critical && !_victimPed.IsInVehicle() && !_cprBeingApplied)
            {
                _critical = true;
                UI.Notify("The ~g~patient~w~ is in critical condition! You will have to CPR him.");
                _victimPed.Task.PlayAnimation("mini@cpr@char_b@cpr_def", "cpr_pumpchest_idle", 8f, -1, true, 8f);
            }

            if (player.IsInRangeOf(_victimPed.Position, 2f) && _critical && !_cprBeingApplied)
            {
                _critical = false;
                _cprBeingApplied = true;
                EngageCPR(player, _victimPed);
            }
            if (_cprBeingApplied)
            {
                int prog = _victimPed.TaskSequenceProgress;
                if (prog == 2)
                {
                    if (_reanimationFailed)
                    {
                        _cprBeingApplied = false;
                        _victimPed.Health = -1;
                        _cprAppliedThisMission = true;
                    }
                    else
                    {
                        _cprBeingApplied = false;
                        _victimPed.Health = 30;
                        _cprAppliedThisMission = true;
                    }
                }
            }
        }
                
        if (_victimPed != null && _onAmbulanceMission && player.IsInVehicle())
        {
            _headsup.Caption = "Level: ~b~" + _level;
            _headsup.Caption += "~w~\nPatient Health: ~b~" + _victimPed.Health + "%~w~";
            _headsup.Draw();
            _headsupRectangle.Draw();
            if (_secondTicks >= 100)
            {
                _secondTicks = 0;
                if (_victimPed.IsInVehicle(player.CurrentVehicle))
                {
                    if (player.CurrentVehicle.Health < _baseDamage)
                    {
                        _victimPed.Health -= _rnd.Next(1, 6);
                        _baseDamage = player.CurrentVehicle.Health;
                    }
                }
                else
                {
                    if (_rnd.Next(0, 101) <= 70 && !_cprBeingApplied && !_cprAppliedThisMission)
                    {
                        _victimPed.Health -= _rnd.Next(1, 3);
                    }
                }
            }
            if (_victimPed.Health <= 0)
            {
                _timer = 200;
                _bigmessage.Caption = "Patient dead";
                _bigmessage.Color = Color.DarkRed;
                _showMessage = true;
                _victimPed.Kill();
                StopAmbulanceMissions();
            }
            if ((_victimPed.Position - player.Position).Length() < 15.0f && player.IsInVehicle() && !_victimPed.IsInVehicle() && !_victimPed.IsGettingIntoAVehicle && _missionState <= 2 && !_critical && !_cprBeingApplied)
            {
                _victimPed.Task.EnterVehicle(player.CurrentVehicle, VehicleSeat.RightRear);
                UI.ShowSubtitle("");
                _baseDamage = player.CurrentVehicle.Health;
            }
            if (player.IsInVehicle() && _victimPed.IsInVehicle(player.CurrentVehicle) && _missionState == 1)
            {
                _missionState = 2;
                UI.ShowSubtitle("Go to the ~b~hospital~w~.", 9999999);
                Vector3 tmpCoords = GetClosestHospital();
                if (_blip.Exists())
                {
                    _blip.Remove();
                }
                    
                _blip = World.CreateBlip(tmpCoords);
                _blip.Color = BlipColor.Blue;
                _blip.ShowRoute = true;
                    
                if (_oldVictim != null)
                {
                    _oldVictim.MarkAsNoLongerNeeded();   
                }
                //this.blip = GTA.Native.Function.Call<int>(GTA.Native.Hash.ADD_BLIP_FOR_COORD, tmpCoords.X, tmpCoords.Y, tmpCoords.Z);
                //GTA.Native.Function.Call(GTA.Native.Hash.SET_BLIP_COLOUR, this.blip, 3);
                //GTA.Native.Function.Call(GTA.Native.Hash.SET_BLIP_ROUTE, this.blip, true);
            }

            float threshold = ( _airMission ? 5.0f : 10.0f );
                
            if (_missionState == 2 && (GetClosestHospital() - player.Position).Length() < threshold && _victimPed.IsInVehicle(player.CurrentVehicle))
            {
                _missionState = 3;
                _oldVictim = _victimPed;
                _oldVictim.Task.LeaveVehicle();
                _oldVictim.Task.GoTo(GetClosestEntrance());

                _blip.ShowRoute = false;
                _blip.Remove();
                  
                _bigmessage.Caption = "Rewarded ~b~" + ((_level * PayoutMultiplier) + (_level * _victimPed.Health)) + "~w~ Dollars";
                _timer = 200;
                _bigmessage.Color = Color.WhiteSmoke;
                _bigmessage.Scale = 1.1f;
                _showMessage = true;
                AddCash((_level * PayoutMultiplier) + (_level * _victimPed.Health));
                _level++;
                //this.victimPed.MarkAsNoLongerNeeded();
                    
                StartAmbulanceMissions();
            }
                
            _secondTicks++;
        }
        else if (_victimPed != null && _onAmbulanceMission && !player.IsInVehicle())
        {
            _headsup.Caption = "Level: ~b~" + _level;
            _headsup.Caption += "\n~w~Patient Health: ~b~" + _victimPed.Health + "%~w~";
            _headsup.Draw();
            _headsupRectangle.Draw();
            if (_secondTicks >= 50)
            {
                _secondTicks = 0;
                if (_victimPed.IsInVehicle())
                {
                    if (_victimPed.CurrentVehicle.Health < _baseDamage)
                    {
                        _victimPed.Health -= _rnd.Next(1, 6);
                        _baseDamage = _victimPed.CurrentVehicle.Health;
                    }
                }
                else
                {
                    if (_rnd.Next(0, 101) <= 50 && !_cprBeingApplied && !_cprAppliedThisMission)
                    {
                        _victimPed.Health -= _rnd.Next(1, 3);
                    }
                }
            }
            if (_victimPed.Health <= 0)
            {
                _timer = 200;
                _bigmessage.Caption = "Patient dead";
                _bigmessage.Color = Color.DarkRed;
                _showMessage = true;

                StopAmbulanceMissions();
            }
            _secondTicks++;
        }
        if (player.Health <= 0)
        {
            StopAmbulanceMissions();
        }
   
    }


    void OnKeyDown(object sender, KeyEventArgs e)
    {
        Ped player = Game.Player.Character;
        
        if (e.KeyCode == Keys.D2)
        {
            if (!_onAmbulanceMission)
            {
                if (player.IsInVehicle())
                {
                    if (player.CurrentVehicle.Model == VehicleHash.Ambulance)
                    {
                        _timer = 200;
                        _bigmessage.Scale = 2.8f;
                        _bigmessage.Caption = "Paramedic";
                        _bigmessage.Color = Color.Goldenrod;
                        _showMessage = true;
                        _airMission = false;
                        StartAmbulanceMissions();
                    }
                    else if (player.CurrentVehicle.Model == VehicleHash.Polmav)
                    {
                        _timer = 200;
                        _bigmessage.Scale = 2.8f;
                        _bigmessage.Caption = "Air Ambulance";
                        _bigmessage.Color = Color.Goldenrod;
                        _showMessage = true;
                        _airMission = true;
                        StartAmbulanceMissions();
                    }
                }
            }
            else
            {
                StopAmbulanceMissions();
            }
        }/*
        else if (e.KeyCode == Keys.D3)
        {
            if (this.victimPed != null)
            {
                this.victimPed.MarkAsNoLongerNeeded();
                this.victimPed.Task.LeaveVehicle();
            }
        }
        else if (e.KeyCode == Keys.I)
        {
            Ped player = Game.Player.Character;
            this.LogToFile(String.Format("new GTA.Math.Vector3({0}f, {1}f, {2}f),", player.Position.X, player.Position.Y, player.Position.Z));
        }*/
    }

    private Vector3 GetClosestHospital()
    {
        Ped player = Game.Player.Character;
        var lastDist = float.MaxValue;
        Vector3 outputDist = new Vector3(0.0f, 0.0f, 0.0f);
        if (!_airMission)
        {
            foreach (var hosp in _hospitals)
            {
                if ((hosp - player.Position).Length() < lastDist)
                {
                    outputDist = hosp;
                    lastDist = (hosp - player.Position).Length();
                }
            }   
        }
        else
        {
            foreach (var hosp in _airHospitals)
            {
                if ((hosp - player.Position).Length() < lastDist)
                {
                    outputDist = hosp;
                    lastDist = (hosp - player.Position).Length();
                }
            }
        }
        return outputDist;
    }

    private Vector3 GetClosestEntrance()
    {
        var lastDist = float.MaxValue;
        Vector3 outputDist = new Vector3(0.0f, 0.0f, 0.0f);
        if (!_airMission)
        {
            foreach (var entr in _hospitalEntrances)
            {
                if ((entr - _oldVictim.Position).Length() < lastDist)
                {
                    outputDist = entr;
                    lastDist = (entr - _oldVictim.Position).Length();
                }
            }
        }
        else
        {
            foreach (var entr in _airHospitalEntrances)
            {
                if ((entr - _oldVictim.Position).Length() < lastDist)
                {
                    outputDist = entr;
                    lastDist = (entr - _oldVictim.Position).Length();
                }
            }
        }
        return outputDist;
    }

    private Vector3 GetRandomVictim()
    {
        Ped player = Game.Player.Character;
        List<Vector3> potentialList = _victims.Where(vict => (vict - player.Position).Length() < 1400.0f).ToList();
        int randChoice = _rnd.Next(0, potentialList.Count);
        return potentialList[randChoice];
    }

    void AddCash(int amount)
    {
        string statNameFull = string.Format("SP{0}_TOTAL_CASH", (Game.Player.Character.Model.Hash == new Model("player_zero").Hash) ? 0 :    //Michael
                                                                (Game.Player.Character.Model.Hash == new Model("player_one").Hash) ? 1 :     //Franklin
                                                                (Game.Player.Character.Model.Hash == new Model("player_two").Hash) ? 2 : 0); //Trevor
        int hash = Function.Call<int>(Hash.GET_HASH_KEY, statNameFull);
        OutputArgument outArg = new OutputArgument();
        Function.Call<bool>(Hash.STAT_GET_INT, hash, outArg, -1);
        var val = outArg.GetResult<int>() + amount;
        Function.Call(Hash.STAT_SET_INT, hash, val, true);
    }

    Vector3 GetSafeRoadPos(Vector3 originalPos)
    {
        OutputArgument outArg = new OutputArgument();
        Function.Call<int>(Hash.GET_CLOSEST_VEHICLE_NODE, originalPos.X, originalPos.Y, originalPos.Z, outArg, 1, 1077936128, 0);
        Vector3 output = outArg.GetResult<Vector3>();
        return output;
    }

    Vector3 GetSafePedPos(Vector3 originalPos)
    {
        OutputArgument out2 = new OutputArgument();
        Function.Call(Hash.GET_SAFE_COORD_FOR_PED, originalPos.X, originalPos.Y, originalPos.Z, 0, out2, 16);
        Vector3 out2Pos = out2.GetResult<Vector3>();
        return out2Pos;
    }

    void EngageCPR(Ped target, Ped victim)
    {
        Vector3 offset = new Vector3(
                                (float)Math.Cos((double)target.Heading * 0.0174532925f + 70f),
                                (float)Math.Sin((double)target.Heading * 0.0174532925f + 70f),
                                0);
        target.Task.ClearAll();
        victim.Task.ClearAllImmediately();
        victim.Position = target.Position + target.ForwardVector * 1.2f + offset * 0.06f;
        victim.Position -= new Vector3(0, 0, victim.HeightAboveGround + 0.2f);
        victim.Heading = target.Heading + 70;

        Random rndGen = new Random();
        if (rndGen.Next(0, 11) >= 9)
            _reanimationFailed = true;
        else
            _reanimationFailed = false;

        //MEDIC
        TaskSequence seq = new TaskSequence();
        seq.AddTask.PlayAnimation("mini@cpr@char_a@cpr_def", "cpr_intro", 8f, 15000, true, 8f);
        seq.AddTask.PlayAnimation("mini@cpr@char_a@cpr_str", "cpr_pumpchest", 8f, 20000, true, 8f);
        if(_reanimationFailed)
            seq.AddTask.PlayAnimation("mini@cpr@char_a@cpr_str", "cpr_fail", 8f, 20000, true, 8f);
        else
            seq.AddTask.PlayAnimation("mini@cpr@char_a@cpr_str", "cpr_success", 8f, 28000, true, 8f);
        seq.Close();
        target.Task.PerformSequence(seq);

        //VICTIM
        TaskSequence seq2 = new TaskSequence();
        seq2.AddTask.PlayAnimation("mini@cpr@char_b@cpr_def", "cpr_intro", 8f, 15000, true, 8f);
        seq2.AddTask.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_pumpchest", 8f, 20000, true, 8f);
        if(_reanimationFailed)
            seq2.AddTask.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_fail", 8f, 20000, true, 8f);
        else
            seq2.AddTask.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_success", 8f, 28000, true, 8f);
        seq2.Close();
        victim.Task.PerformSequence(seq2);
    }

    /*
    private void LogToFile(string text)
    {
        using (StreamWriter w = File.AppendText("log.txt"))
        {
            w.Write(text + "\n");
        }
    }*/

    private void StartAmbulanceMissions() 
    {
        Ped player = Game.Player.Character;
       
        _onAmbulanceMission = true;
        _missionState = 1;
        _critical = false;
        _cprAppliedThisMission = false;
        _cprBeingApplied = false;
        _reanimationFailed = false;
            
        _headsup = new UIText("Level: ~b~" + _level, new Point(2, 320), 0.7f, Color.WhiteSmoke, GTA.Font.HouseScript, false);
        _headsupRectangle = new UIRectangle(new Point(0, 320), new Size(215, 65), Color.FromArgb(100, 0, 0, 0));
        /*Function.Call(Hash._0xB87A37EEB7FAA67D, "STRING");
        Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, "Pick up the ~g~patient~w~.");
        Function.Call(Hash._0x9D77056A530643F6, 0.5f, 1);*/
        UI.ShowSubtitle("Pick up the ~g~patient~w~.", 9999999);

        Vector3 pedSpawnPoint = GetRandomVictim();

        _victimPed = Function.Call<Ped>(Hash.CREATE_RANDOM_PED, pedSpawnPoint.X, pedSpawnPoint.Y, pedSpawnPoint.Z);

        _victimPed.IsPersistent = true;
        _victimPed.AlwaysDiesOnLowHealth = false;
        

        //this.victimPed.IsPersistent = true;
        //this.victimPed.AlwaysDiesOnLowHealth = false;
        _victimPed.Health = _level >= 14 ? 30 : _difficultyScale[_level-1];
        player.IsEnemy = false;
        _victimPed.IsEnemy = false;

        Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, _victimPed.Handle,
            _victimPed.Gender == Gender.Male ? "move_m@injured" : "move_f@injured");

        if (_blip != null)
        {
            if (_blip.Exists())
            {
                _blip.Remove();
            }
        }
        
        _blip = _victimPed.AddBlip();
        Function.Call(Hash.SET_BLIP_AS_FRIENDLY, _blip.Handle, true);
        _blip.Color = BlipColor.Green;
        _blip.ShowRoute = true;

    }

    private void StopAmbulanceMissions()
    {
        if (_onAmbulanceMission)
        {
            _onAmbulanceMission = false;
            _missionState = 0;
            _level = 1;
            //this.headsup = null;

            if(_victimPed !=null)
                _victimPed.MarkAsNoLongerNeeded();
            if(_oldVictim != null)
                _oldVictim.MarkAsNoLongerNeeded();
            UI.ShowSubtitle("");
            if (_blip.Exists() && _blip != null)
            {
                _blip.ShowRoute = false;
                _blip.Remove();
            }
        }
        //this.victimPed = null;
        //GTA.Native.Function.Call(GTA.Native.Hash.DELETE_PED, this.victimPed);
    }
}