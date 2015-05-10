using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using GTA;
using GTAModExperimenting;
using System.IO;

public class GTAAmbulance : Script
{
    
    public GTAAmbulance()
    {
        KeyDown += OnKeyDown;
        Tick += OnTick;
        Interval = 1;
        //this.coordLabel = new UIText("PlaceHolder", new Point(10, 10), 0.5f, Color.White, 4, false);
        this.bigmessage = new UIText("PARAMEDIC", new Point(630, 100), 2.8f, Color.Goldenrod, 2, true);
    }
    private const int payoutMultiplier = 100;

    //private UIText coordLabel = null;
    private UIText headsup = null;
    private UIRectangle headsupRectangle = null;
    private UIText bigmessage;
    private Boolean onMission = false;
    private Ped victimPed = null;
    private Ped oldVictim = null;
    private Blip blip;
    private Random rnd = new Random();

    private int secondTicks = 0;
    private int ticks = 0;
    private int level = 12;
    private int timer = 1000;
    private bool showMessage = false;
    private int[] difficultyScale = new int[] {100, 90, 85, 80, 75, 70, 60, 55, 50, 45, 40, 35, 30};
    private int baseDamage = 900;

    private int missionState = 0; // 0 - not started, 1 - going to patient, 2 - going to hospital, 3 - in hospital
    private bool airMission = false;
    //private GTA.Math.Vector3 hospital = new GTA.Math.Vector3(1841.359f, 3668.857f, 33.679f);
    private GTA.Math.Vector3[] Hospitals = new GTA.Math.Vector3[] {
        new GTA.Math.Vector3(-454.8831f, -340.2435f, 34.36343f),
        new GTA.Math.Vector3(1842.389f, 3667.902f, 33.67993f),
        new GTA.Math.Vector3(300.457f, -1441.283f, 29.79361f),
        new GTA.Math.Vector3(-238.5425f, 6334.082f, 32.41678f),
        new GTA.Math.Vector3(364.4797f, -592.1211f, 28.68582f),
    };

    private GTA.Math.Vector3[] HospitalEntrances = new GTA.Math.Vector3[] {
        new GTA.Math.Vector3(-448.5311f, -347.6872f, 34.50179f),
        new GTA.Math.Vector3(294.4863f, -1448.287f, 29.9666f),
        new GTA.Math.Vector3(359.7662f, -584.9894f, 28.81874f),
        new GTA.Math.Vector3(1839.238f, 3673.694f, 34.27666f),
        new GTA.Math.Vector3(-248.1741f, 6330.686f, 32.4262f),
    };

    private GTA.Math.Vector3[] AirHospitals = new GTA.Math.Vector3[] {
        new GTA.Math.Vector3(313.6914f, -1464.372f, 46.89649f),
        new GTA.Math.Vector3(352.8871f, -588.515f, 74.55135f),
    };

    private GTA.Math.Vector3[] AirHospitalEntrances = new GTA.Math.Vector3[] {
        new GTA.Math.Vector3(391.1208f, -1432.876f, 29.43557f),
        new GTA.Math.Vector3(339.185f, -584.1078f, 74.16563f),
    };

    private GTA.Math.Vector3[] Victims = new GTA.Math.Vector3[] {
        new GTA.Math.Vector3(-155.1335f, 6264.229f, 31.48946f), //PALETO BAY
        new GTA.Math.Vector3(-268.0224f, 6180.009f, 31.39273f),
        new GTA.Math.Vector3(-283.2744f, 6223.528f, 31.4958f),
        new GTA.Math.Vector3(-78.92438f, 6489.741f, 31.00436f),
        new GTA.Math.Vector3(-52.61729f, 6526.674f, 31.02834f),
        new GTA.Math.Vector3(175.5626f, 7040.976f, 1.468205f),
        new GTA.Math.Vector3(-91.19389f, 6337.676f, 31.03658f), //Sandy Shores
        new GTA.Math.Vector3(-100.5584f, 6411.113f, 31.02189f),
        new GTA.Math.Vector3(-78.92438f, 6489.741f, 31.00436f),
        new GTA.Math.Vector3(-52.61729f, 6526.674f, 31.02834f),
        new GTA.Math.Vector3(54.18509f, 7085.758f, 2.171395f),
        new GTA.Math.Vector3(175.5626f, 7040.976f, 1.468205f),
        new GTA.Math.Vector3(-582.409f, 5315.361f, 69.99908f),
        new GTA.Math.Vector3(-1633.15f, 4737.142f, 53.29347f),
        new GTA.Math.Vector3(-124.679f, 4273.219f, 45.12233f),
        new GTA.Math.Vector3(2094.326f, 4769.768f, 40.95844f),
        new GTA.Math.Vector3(1963.848f, 5168.533f, 47.40821f),      
        new GTA.Math.Vector3(2633.842f, 4527.676f, 36.76321f),
        new GTA.Math.Vector3(1848.775f, 3924.496f, 32.77434f),
        new GTA.Math.Vector3(1823.735f, 3766.912f, 33.16585f),
        new GTA.Math.Vector3(1706.295f, 3749.585f, 33.75248f),
        new GTA.Math.Vector3(1351.814f, 3593.798f, 34.63599f),
        new GTA.Math.Vector3(1993.938f, 3060.152f, 46.81282f),
        new GTA.Math.Vector3(2567.84f, 384.4785f, 108.2305f), 
        new GTA.Math.Vector3(-1032.054f, -2725.19f, 13.16502f), //Los Santos
		new GTA.Math.Vector3(-682.0075f, -2233.63f, 5.491331f),
		new GTA.Math.Vector3(-396.4651f, -2270.628f, 7.144282f),
		new GTA.Math.Vector3(59.72925f, -2562.665f, 5.499238f),
		new GTA.Math.Vector3(229.4973f, -3113.489f, 5.328383f),
		new GTA.Math.Vector3(824.1693f, -2981.625f, 5.618261f),
		new GTA.Math.Vector3(806.4105f, -2131.054f, 28.87451f),
		new GTA.Math.Vector3(358.6248f, -1980.638f, 23.81694f),
		new GTA.Math.Vector3(88.3496f, -1508.658f, 28.87959f),
		new GTA.Math.Vector3(471.5156f, -1519.8f, 28.83399f),
		new GTA.Math.Vector3(714.605f, -1077.308f, 21.82764f),
		new GTA.Math.Vector3(227.6832f, -957.8099f, 28.8784f),
		new GTA.Math.Vector3(151.8911f, -1033.908f, 28.88036f),
		new GTA.Math.Vector3(-57.86221f, -789.1469f, 43.77345f),
		new GTA.Math.Vector3(258.0165f, -635.2186f, 40.29241f),
		new GTA.Math.Vector3(247.5053f, -380.2834f, 44.06373f),
		new GTA.Math.Vector3(165.3261f, -114.6701f, 61.74897f),
		new GTA.Math.Vector3(237.3834f, -34.43047f, 69.25822f),
		new GTA.Math.Vector3(546.7581f, -206.9675f, 53.62393f),
		new GTA.Math.Vector3(632.5165f, 99.35558f, 88.50195f),
		new GTA.Math.Vector3(883.628f, -133.382f, 77.66323f),
		new GTA.Math.Vector3(1050.944f, -490.6486f, 63.45192f),
		new GTA.Math.Vector3(1126.734f, -655.963f, 56.29839f),
		new GTA.Math.Vector3(452.8423f, 126.7028f, 98.89735f),
		new GTA.Math.Vector3(229.1809f, 341.4336f, 105.08f),
		new GTA.Math.Vector3(-102.4168f, -120.8662f, 57.2532f),
		new GTA.Math.Vector3(-356.6556f, 33.16491f, 47.33154f),
		new GTA.Math.Vector3(-676.6347f, 299.3956f, 81.59211f),
		new GTA.Math.Vector3(-891.4418f, -2.590907f, 42.91672f),
		new GTA.Math.Vector3(-1419.533f, -197.3691f, 46.74275f),
		new GTA.Math.Vector3(-1862.799f, -353.8355f, 48.7586f),
		new GTA.Math.Vector3(-1887.052f, -570.1543f, 11.27715f),
		new GTA.Math.Vector3(-1852.131f, -1223.636f, 12.54064f),
		new GTA.Math.Vector3(-1279.246f, -1203.424f, 4.349295f),
		new GTA.Math.Vector3(-708.0892f, -1397.278f, 4.671808f),
		new GTA.Math.Vector3(-532.0626f, -1218.669f, 17.96088f),
		new GTA.Math.Vector3(-603.4581f, -947.6642f, 21.64981f),
		new GTA.Math.Vector3(-1092.189f, 433.6045f, 74.99454f),
		new GTA.Math.Vector3(-747.6935f, 815.9718f, 212.9589f),
    };

    void OnTick(object sender, EventArgs e)
    {
            if (this.ticks >= this.timer)
            {
                this.ticks = 0;
                this.showMessage = false;
            }
            if (this.showMessage)
            {
                this.bigmessage.Draw();
                this.ticks += 1;
            }
            Ped player = Game.Player.Character;
            
            //this.coordLabel.Text = String.Format("Revision: {0}", Revision);    

            //this.coordLabel.Draw();
            if (this.victimPed != null && this.onMission && player.IsInVehicle())
            {
                headsup.Text = "Level: " + this.level.ToString();
                headsup.Text += "\nPatient Health: " + this.victimPed.Health.ToString() + "%";
                headsup.Draw();
                headsupRectangle.Draw();
                if (secondTicks >= 100)
                {
                    secondTicks = 0;
                    if (this.victimPed.IsInVehicle(player.CurrentVehicle))
                    {
                        if (player.CurrentVehicle.Health < this.baseDamage)
                        {
                            victimPed.Health -= rnd.Next(1, 6);
                            this.baseDamage = player.CurrentVehicle.Health;
                        }
                    }
                    else
                    {
                        if (rnd.Next(0, 101) <= 70)
                        {
                            victimPed.Health -= rnd.Next(1, 3);
                        }
                    }
                }
                if (this.victimPed.Health <= 0)
                {
                    this.timer = 200;
                    this.bigmessage.Text = "Patient dead";
                    this.bigmessage.Color = Color.DarkRed;
                    this.showMessage = true;
                    this.victimPed.Kill();
                    this.StopAmbulanceMissions();
                }
                if ((this.victimPed.Position - player.Position).Length() < 15.0f && player.IsInVehicle() && !victimPed.IsInVehicle() && !victimPed.IsGettingIntoAVehicle && this.missionState <= 2)
                {
                    this.victimPed.Task.EnterVehicle(player.CurrentVehicle, GTA.VehicleSeat.RightRear);
                    GTA.Native.Function.Call(GTA.Native.Hash._0xB87A37EEB7FAA67D, "STRING");
                    GTA.Native.Function.Call(GTA.Native.Hash._ADD_TEXT_COMPONENT_STRING, "");
                    GTA.Native.Function.Call(GTA.Native.Hash._0x9D77056A530643F6, 0.5f, 1);
                    //GTA.UI.ShowSubtitle(""); //Next version
                    this.baseDamage = player.CurrentVehicle.Health;
                }
                if (player.IsInVehicle() && victimPed.IsInVehicle(player.CurrentVehicle) && this.missionState == 1)
                {
                    this.missionState = 2;
                    GTA.Native.Function.Call(GTA.Native.Hash._0xB87A37EEB7FAA67D, "STRING");
                    GTA.Native.Function.Call(GTA.Native.Hash._ADD_TEXT_COMPONENT_STRING, "Go to the ~b~hospital~w~.");
                    GTA.Native.Function.Call(GTA.Native.Hash._0x9D77056A530643F6, 0.5f, 1);
                    //GTA.UI.ShowSubtitle("Go to the ~b~hospital~w~.");
                    GTA.Math.Vector3 tmpCoords = this.GetClosestHospital();
                    if (this.blip.Exists)
                    {
                        //GTAModExperimenting.Blip.Delete(this.blip.ID);
                        this.blip.Alpha = 0.0f;
                    }
                    
                    this.blip = GTAModExperimenting.Blip.Create(tmpCoords);
                    this.blip.Colour = 3;
                    this.blip.Route = true;
                    if (this.oldVictim != null)
                    {
                        this.oldVictim.MarkAsNoLongerNeeded();   
                    }
                    //this.blip = GTA.Native.Function.Call<int>(GTA.Native.Hash.ADD_BLIP_FOR_COORD, tmpCoords.X, tmpCoords.Y, tmpCoords.Z);
                    //GTA.Native.Function.Call(GTA.Native.Hash.SET_BLIP_COLOUR, this.blip, 3);
                    //GTA.Native.Function.Call(GTA.Native.Hash.SET_BLIP_ROUTE, this.blip, true);
                }

                float threshold = ( this.airMission ? 5.0f : 10.0f );
                
                if (this.missionState == 2 && (this.GetClosestHospital() - player.Position).Length() < threshold && this.victimPed.IsInVehicle(player.CurrentVehicle))
                {
                    this.missionState = 3;
                    this.oldVictim = this.victimPed;
                    this.oldVictim.Task.LeaveVehicle();
                    this.oldVictim.Task.GoTo(GetClosestEntrance());

                    this.blip.Route = false;
                    this.blip.Alpha = 0.0f;
                  
                    
                    this.bigmessage.Text = "Rewarded " + ((this.level * payoutMultiplier) + (this.level * victimPed.Health)).ToString();
                    this.timer = 200;
                    this.bigmessage.Color = Color.WhiteSmoke;
                    this.bigmessage.Size = 1.1f;
                    this.showMessage = true;
                    AddCash((this.level * payoutMultiplier) + (this.level * victimPed.Health));
                    this.level++;
                    //this.victimPed.MarkAsNoLongerNeeded();
                    
                    this.StartAmbulanceMissions();
                }
                
                secondTicks++;
            }
            else if (this.victimPed != null && this.onMission && !player.IsInVehicle())
            {
                headsup.Text = "Level: " + this.level.ToString();
                headsup.Text += "\nPatient Health: " + this.victimPed.Health.ToString() + "%";
                headsup.Draw();
                headsupRectangle.Draw();
                if (secondTicks >= 50)
                {
                    secondTicks = 0;
                    if (this.victimPed.IsInVehicle())
                    {
                        if (victimPed.CurrentVehicle.Health < this.baseDamage)
                        {
                            victimPed.Health -= rnd.Next(1, 6);
                            this.baseDamage = victimPed.CurrentVehicle.Health;
                        }
                    }
                    else
                    {
                        if (rnd.Next(0, 101) <= 50)
                        {
                            victimPed.Health -= rnd.Next(1, 3);
                        }
                    }
                }
                if (this.victimPed.Health <= 0)
                {
                    this.timer = 200;
                    this.bigmessage.Text = "Patient dead";
                    this.bigmessage.Color = Color.DarkRed;
                    this.showMessage = true;

                    this.StopAmbulanceMissions();
                }
                secondTicks++;
            }
            if (player.Health <= 0)
            {
                StopAmbulanceMissions();
            }
   
    }

    void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.D2)
        {
            if (!this.onMission)
            {
                Ped player = Game.Player.Character;
                if (player.CurrentVehicle.Model == GTA.Native.VehicleHash.Ambulance)
                {
                    this.timer = 200;
                    this.bigmessage.Size = 2.8f;
                    this.bigmessage.Text = "Paramedic";
                    this.bigmessage.Color = Color.Goldenrod;
                    this.showMessage = true;
                    this.airMission = false;
                    this.StartAmbulanceMissions();
                }
                else if (player.CurrentVehicle.Model == GTA.Native.VehicleHash.Polmav)
                {
                    this.timer = 200;
                    this.bigmessage.Size = 2.8f;
                    this.bigmessage.Text = "Air Ambulance";
                    this.bigmessage.Color = Color.Goldenrod;
                    this.showMessage = true;
                    this.airMission = true;
                    this.StartAmbulanceMissions();
                }
            }
            else
            {
                this.StopAmbulanceMissions();
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

    private GTA.Math.Vector3 GetClosestHospital()
    {
        Ped player = Game.Player.Character;
        var lastDist = float.MaxValue;
        GTA.Math.Vector3 outputDist = new GTA.Math.Vector3(0.0f, 0.0f, 0.0f);
        if (!this.airMission)
        {
            foreach (var hosp in Hospitals)
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
            foreach (var hosp in AirHospitals)
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

    private GTA.Math.Vector3 GetClosestEntrance()
    {
        var lastDist = float.MaxValue;
        GTA.Math.Vector3 outputDist = new GTA.Math.Vector3(0.0f, 0.0f, 0.0f);
        if (!this.airMission)
        {
            foreach (var entr in HospitalEntrances)
            {
                if ((entr - this.oldVictim.Position).Length() < lastDist)
                {
                    outputDist = entr;
                    lastDist = (entr - this.oldVictim.Position).Length();
                }
            }
        }
        else
        {
            foreach (var entr in AirHospitalEntrances)
            {
                if ((entr - this.oldVictim.Position).Length() < lastDist)
                {
                    outputDist = entr;
                    lastDist = (entr - this.oldVictim.Position).Length();
                }
            }
        }
        return outputDist;
    }

    private GTA.Math.Vector3 GetRandomVictim()
    {
        Ped player = Game.Player.Character;
        List<GTA.Math.Vector3> potentialList = new List<GTA.Math.Vector3>();;
        foreach (var vict in Victims)
        {
            if ((vict - player.Position).Length() < 1400.0f) //Threshold
            {
                potentialList.Add(vict);
            }
        }
        return potentialList[rnd.Next(0, potentialList.Count)];
    }

    void AddCash(int amount)
    {
        string statNameFull = string.Format("SP{0}_TOTAL_CASH", (Game.Player.Character.Model.Hash == new Model("player_zero").Hash) ? 0 :    //Michael
                                                                (Game.Player.Character.Model.Hash == new Model("player_one").Hash) ? 1 :     //Franklin
                                                                (Game.Player.Character.Model.Hash == new Model("player_two").Hash) ? 2 : 0); //Trevor
        int hash = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_HASH_KEY, statNameFull);
        int val = 0;
        GTA.Native.OutputArgument outArg = new GTA.Native.OutputArgument();
        GTA.Native.Function.Call<bool>(GTA.Native.Hash.STAT_GET_INT, hash, outArg, -1);
        val = outArg.GetResult<int>() + amount;
        GTA.Native.Function.Call(GTA.Native.Hash.STAT_SET_INT, hash, val, true);
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
       
        this.onMission = true;
        this.missionState = 1;

            
        this.headsup = new UIText("Level: " + this.level.ToString(), new Point(2, 520), 0.7f, Color.WhiteSmoke, 1, false);
        this.headsupRectangle = new UIRectangle(new Point(0, 520), new Size(215, 65), Color.FromArgb(100, 0, 0, 0));
        GTA.Native.Function.Call(GTA.Native.Hash._0xB87A37EEB7FAA67D, "STRING");
        GTA.Native.Function.Call(GTA.Native.Hash._ADD_TEXT_COMPONENT_STRING, "Pick up the ~g~patient~w~.");
        GTA.Native.Function.Call(GTA.Native.Hash._0x9D77056A530643F6, 0.5f, 1);
        //GTA.UI.ShowSubtitle("Pick up the ~g~patient~w~.");

        GTA.Math.Vector3 pedSpawnPoint = GetRandomVictim();
        
        this.victimPed = GTA.Native.Function.Call<Ped>(GTA.Native.Hash.CREATE_RANDOM_PED, pedSpawnPoint.X, pedSpawnPoint.Y, pedSpawnPoint.Z);

        this.victimPed.IsPersistent = true;
        this.victimPed.AlwaysDiesOnLowHealth = false;
        if (this.level >= 14)
        {
            this.victimPed.Health = 30;
        }
        else
        {
            this.victimPed.Health = this.difficultyScale[this.level-1];
        }
        //GTA.Native.Function.Call(GTA.Native.Hash.SET_PED_ALTERNATE_MOVEMENT_ANIM, new GTA.Native.InputArgument(this.victimPed.ID));
        player.IsEnemy = false;
        victimPed.IsEnemy = false;

        if (this.blip != null)
        {
            if (this.blip.Exists)
            {
                //GTAModExperimenting.Blip.Delete(this.blip.ID);
                this.blip.Alpha = 0.0f;
            }
        }
        //GTA.Native.Function.Call(GTA.Native.Hash.REMOVE_BLIP, this.blip);
        //this.blip = GTA.Native.Function.Call<int>(GTA.Native.Hash.ADD_BLIP_FOR_ENTITY, this.victimPed);
        this.blip = GTAModExperimenting.Blip.Create(this.victimPed);
        GTA.Native.Function.Call(GTA.Native.Hash.SET_BLIP_AS_FRIENDLY, this.blip.ID, true);
        this.blip.Colour = 2;

        //GTA.Native.Function.Call(GTA.Native.Hash.SET_BLIP_AS_FRIENDLY, this.blip, true);
        //GTA.Native.Function.Call(GTA.Native.Hash.SET_BLIP_COLOUR, this.blip, 2);
        //BLIPS ARE NOT SUPPORTED YET
    }

    private void StopAmbulanceMissions()
    {
        this.onMission = false;
        this.missionState = 0;
        this.level = 1;
        //this.headsup = null;
        GTA.Native.Function.Call(GTA.Native.Hash._0xB87A37EEB7FAA67D, "STRING");
        GTA.Native.Function.Call(GTA.Native.Hash._ADD_TEXT_COMPONENT_STRING, "");
        GTA.Native.Function.Call(GTA.Native.Hash._0x9D77056A530643F6, 0.5f, 1);
        //GTA.UI.ShowSubtitle("");
        //GTA.Native.Function.Call(GTA.Native.Hash.SET_BLIP_ROUTE, this.blip, false);
        //GTA.Native.Function.Call(GTA.Native.Hash.REMOVE_BLIP, this.blip);
        this.victimPed.MarkAsNoLongerNeeded();
        this.oldVictim.MarkAsNoLongerNeeded();
        if (this.blip.Exists && this.blip != null)
        {
            this.blip.Route = false;
            this.blip.Alpha = 0.0f;
            //GTAModExperimenting.Blip.Delete(this.blip.ID);
        }
        //this.victimPed = null;
        //GTA.Native.Function.Call(GTA.Native.Hash.DELETE_PED, this.victimPed);
    }
}


//Hospital:
//Pedspawn: 2593.156f, 3160.262f, 50.3f