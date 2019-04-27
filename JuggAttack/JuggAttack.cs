using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Timers;
using System.IO;
using InfinityScript;
using static InfinityScript.GSCFunctions;

namespace JuggAttack
{
    public class JuggAttack : BaseScript
    {
        private Entity _airdropCollision;
        private int Crate_FX;
        private int Health_FX;
        private int thermiteExplosion;
        private int thermiteFire;
        private int thermiteEmbers;
        private int thermiteTrail;
        public int _mapCount = 0;
        public int curObjID;
        public int EMP;
        public bool JuggSpawnsStarted = false;
        public Timer ModeTimer;
        public int GameTime;
        private int BloodFX;
        public string endMessage;
        public bool GameEnded = false;
        public int LastJuggActive;
        public int JuggGlobalHealth = 800;
        public Vector3[] JuggSpawns = { new Vector3(-875.2101f, -138.9879f, -302.8499f), new Vector3(299.1145f, 1250.159f, 17.13539f), new Vector3(-542.4986f, 1590.532f, -114.8608f), new Vector3(360.6809f, 2059.878f, 17.13553f), new Vector3(1044.177f, 1074.527f, 17.15072f), new Vector3(1433.434f, 699.8627f, -154.3688f), new Vector3(1109.575f, 222.4743f, -187.8495f), new Vector3(758.2672f, -876.1357f, -268.3625f), new Vector3(-168.4944f, 1254.475f, -194.0723f), new Vector3(92.51382f, 616.7653f, -171.8359f), new Vector3(252.925f, -378.0197f, -202.6986f), new Vector3(366.9439f, -117.4409f, -174.3647f), new Vector3(791.052f, 2043.787f, -126.8646f), new Vector3(359.3178f, 573.0622f, -181.8649f), new Vector3(784.2098f, 847.5285f, -218.7644f), new Vector3(-851.0242f, 487.0947f, -279.8683f), new Vector3(-374.8155f, -117.2774f, -194.3652f), new Vector3(457.126f, 130.8158f, -194.3623f), new Vector3(-593.5805f, -219.9054f, -308.866f), new Vector3(-1007.44f, 994.5746f, -224.5692f), new Vector3(1177.246f, -315.583f, -150.1503f) };
        public Vector3[] JuggAngles = { new Vector3(0, 41.11072f, 0), new Vector3(0, -141.7491f, 0), new Vector3(0, -35.06085f, 0), new Vector3(0, 175.591f, 0), new Vector3(0, -138.596f, 0), new Vector3(0, -90.61871f, 0), new Vector3(0, -108.6583f, 0), new Vector3(0, 88.65619f, 0), new Vector3(0, -120.1994f, 0), new Vector3(0, 162.9622f, 0), new Vector3(0, -20.92368f, 0), new Vector3(0, -19.11642f, 0), new Vector3(0, 177.1104f, 0), new Vector3(0, -36.43088f, 0), new Vector3(0, 85.4954f, 0), new Vector3(0, -67.45627f, 0), new Vector3(0, 112.5492f, 0), new Vector3(0, -62.40256f, 0), new Vector3(0, 85.4954f, 0), new Vector3(0, 18.64916f, 0), new Vector3(0, 125.0627f, 0) };
        public string[] IdleAnims = { "pb_stand_alert", "pb_hold_idle", "pb_stand_alert_RPG", "pb_stand_alert_pistol", "pb_stand_shellshock", "pb_stand_alert_mg" };
        public string RunAnim = "pb_sprint_pistol";
        public string WalkAnim = "pb_stand_shoot_walk_forward_unarmed";
        public string FallAnim = "pb_runjump_takeoff_pistol";
        public string LandAnim = "pb_runjump_land_pistol";
        public string LadderAnim = "pb_climbdown";
        public string ClimbAnim = "mp_mantle_up_57";
        public string[] DeathAnims = { "pb_stand_death_frontspin", "pb_stand_death_headchest_topple" };
        public string RunHitAnim = "pb_stumble_pistol_forward";
        public string WalkHitAnim = "pb_stumble_pistol_walk_forward";
        public string AttackAnim = "pt_melee_right2right_2";
        public string[] HurtAnims = { "pb_stumble_pistol_walk_forward", "pb_stumble_pistol_forward" };
        public string LoseAnim = "pb_chicken_dance_crouch";
        public Entity[] Juggs = new Entity[21];
        public List<Entity> SpawnedJuggs = new List<Entity>();
        public int JuggsKilled = 0;
        public int forceBoss = 0;
        public string[] encouragment = {"Excellent work!", "Let's hope the Juggs don't return...", "The Juggs may not be gone for long though...", "We're Juggernaut free! At least, for now.", "Beware the Juggs' next attack!", "We showed those Juggs who's boss!", "The Juggs are planning another attack as we speak...", "Think of the baby Juggs."};
        //public Entity crate = null;
        private int crateCount = 0;
        public List<string> deadPlayers = new List<string>();

        public JuggAttack()
        {
            if (GetDvar("mapname") != "mp_dome") return;

            PreCacheItem("at4_mp");
            PreCacheShader("death_nuke");
            curObjID = 31 - _mapCount++;
            Entity care_package = GetEnt("care_package", "targetname");
            _airdropCollision = GetEnt(care_package.Target, "targetname");
            Crate_FX = LoadFX("smoke/signal_smoke_airdrop");
            Health_FX = LoadFX("misc/flare_ambient_green");
            thermiteExplosion = LoadFX("explosions/powerlines_c");
            thermiteFire = LoadFX("fire/firelp_med_pm_nodistort");
            thermiteEmbers = LoadFX("explosions/powerlines_c");
            thermiteTrail = LoadFX("smoke/jet_contrail");
            SetDvar("ui_allow_teamchange", "0");
            BloodFX = LoadFX("impacts/flesh_hit");
            EMP = LoadFX("explosions/emp_flash_mp");
            PrecacheAnims();

            GameTime = 0;
            endMessage = GetRandomEncourageMessage();
            ModeTimer = new Timer(1000);
            ModeTimer.Elapsed += TimerTest;
            SpawnJuggs();
            AntiCheat();

            Notified += onGlobalNotify;

            PlayerConnected += new Action<Entity>(e =>
            {
                Entity entity = e;
                entity.SetClientDvar("ui_allow_teamchange", "0");
                entity.SetCanDamage(false);
                entity.SetField("killstreak", 0);
                entity.SetPlayerData("killstreaksState", "countToNext", 5);
                entity.SetPlayerData("killstreaksState", "nextIndex", 1);
                entity.SetPlayerData("killstreaksState", "numAvailable", 2);
                entity.SetPlayerData("killstreaksState", "icons", 1, -1);
                entity.SetPlayerData("killstreaksState", "hasStreak", 1, false);
                entity.SetPlayerData("killstreaksState", "icons", 2, 20);
                entity.SetPlayerData("killstreaksState", "hasStreak", 2, false);
                entity.SetClientDvar("cg_objectiveText", "Survive the Juggernauts' attacks...");
                entity.SetPerk("specialty_extendedmelee", true, true);
                if (!ModeTimer.Enabled)
                    ModeTimer.Enabled = true;
                if (deadPlayers.Contains(entity.Name))
                    AfterDelay(2000, () => entity.Notify("menuresponse", "team_marinesopfor", "spectator"));
                OnInterval(500, () =>
                {
                    int playersAlive = GetTeamPlayersAlive("axis");
                    if (playersAlive < 1)
                    {
                        ModeTimer.Enabled = false;
                        ModeTimer.Dispose();
                        EndGameHUD(entity);
                        Notify("gameLost");
                        return false;
                    }
                    else return true;
                });
                entity.SetMoveSpeedScale(1f);
                entity.TakeWeapon("iw5_smaw_mp");
                entity.GiveWeapon("at4_mp");
                //entity.SetWeaponHudIconOverride("secondaryoffhand", "death_nuke");
                entity.OnNotify("weapon_change", (ent, wep) =>
                {
                    entity.SetMoveSpeedScale(1f);
                    if (((string)wep == "trophy_mp" || (string)wep == "scrambler_mp") && crateCount > 0)
                    {
                        entity.IPrintLnBold("Airspace is resuppling. Try again later.");
                        AfterDelay(50, () =>
                        entity.SwitchToWeapon("iw5_fad_mp_reflex_xmags_camo11"));
                        return;
                    }
                });

                entity.SetModel("mp_body_ally_ghillie_desert_sniper");
                entity.SetViewModel("viewhands_iw5_ghillie_desert");
                //entity.SetField("maxhealth", 999999);
                entity.Health = 999999;//Set health high to avoid Teamkilling
                entity.SpawnedPlayer += () => OnPlayerSelectedAsJugg(entity);
                HudElem counter = HudElem.CreateFontString(entity, HudElem.Fonts.HudSmall, 1.5f);
                counter.SetPoint("BOTTOM CENTER", "BOTTOM CENTER");
                HudElem combatHighFeedback = NewClientHudElem(entity);
                combatHighFeedback.HorzAlign = HudElem.HorzAlignments.Center;
                combatHighFeedback.VertAlign = HudElem.VertAlignments.Middle;
                combatHighFeedback.X = -12;
                combatHighFeedback.Y = -12;
                combatHighFeedback.Alpha = 0;
                combatHighFeedback.Archived = true;
                combatHighFeedback.SetShader("damage_feedback", 24, 48);
                entity.SetField("hud_damagefeedback", combatHighFeedback);
                entity.NotifyOnPlayerCommand("triggeruse", "+activate");
                OnInterval(1000, () =>
                    {
                        counter.SetText("Juggernauts Left: " + (Juggs.Length - JuggsKilled).ToString());
                        return true;
                    });
                counter.Alpha = 1;
                counter.HideWhenInMenu = true;
            });
        }

        public void onGlobalNotify(int entRef, string message, Parameter[] param)
        {
            if (message != "grenade_fire") return;
            string weaponName = (string)param[1];
            Entity grenade = (Entity)param[0];
            Entity entity = Entity.GetEntity(entRef);
            if (weaponName == "scrambler_mp")
            {
                AfterDelay(50, () =>
                    entity.SwitchToWeapon("iw5_fad_mp_reflex_xmags_camo11"));
                AfterDelay(700, () => entity.TakeWeapon("scrambler_mp"));

                entity.SetPlayerData("killstreaksState", "hasStreak", 2, false);
                //entity.SetPlayerData("killstreaksState", "icons", 0, 0);

                //Vector3 playerForward = entity.GetTagOrigin("tag_weapon") + Call<Vector3>("AnglesToForward", entity.Call<Vector3>("getplayerangles")) * 100000;
                //Vector3 dropZone = Call<Vector3>("AnglesToForward", new Vector3(0, entity.Call<Vector3>("getplayerangles").Y, 0)) * 12;


                //Entity refobject = Spawn("script_model", ent.Origin + new Vector3(0, 0, 10) + new Vector3(dropZone.X, dropZone.Y, 0));
                //refobject.SetModel("tag_origin");
                //refobject.SetField("angles", ent.Call<Vector3>("getplayerangles"));
                //refobject.MoveTo(playerForward, 100);
                //refobject.Hide();
                //refobject.Call(33351, refobject.Origin, playerForward * 1000);

                //Call("playfxontag", Health_FX, refobject, "tag_origin");

                grenade.SetField("type", "health");
                grenade.Notify("death");
                watchForMarkerStuck(grenade);
            }

            else if (weaponName == "trophy_mp")
            {
                AfterDelay(50, () =>
                    entity.SwitchToWeapon("iw5_fad_mp_reflex_xmags_camo11"));
                AfterDelay(700, () => entity.TakeWeapon("trophy_mp"));

                entity.SetPlayerData("killstreaksState", "hasStreak", 1, false);
                //entity.SetPlayerData("killstreaksState", "icons", 0, 0);

                //Vector3 playerForward = entity.GetTagOrigin("tag_weapon") + Call<Vector3>("AnglesToForward", entity.Call<Vector3>("getplayerangles")) * 100000;
                //Vector3 dropZone = Call<Vector3>("AnglesToForward", new Vector3(0, entity.Call<Vector3>("getplayerangles").Y, 0)) * 12;


                //Entity refobject = Spawn("script_model", ent.Origin + new Vector3(0, 0, 40) + new Vector3(dropZone.X, dropZone.Y, 0));
                //refobject.SetModel("tag_origin");
                //refobject.SetField("angles", ent.Call<Vector3>("getplayerangles"));
                //refobject.MoveTo(playerForward, 100);
                //refobject.Hide();
                //refobject.Call(33351, refobject.Origin, playerForward * 1000);

                grenade.SetField("type", "ammo");
                grenade.Notify("death");
                watchForMarkerStuck(grenade);
            }
            else if (weaponName == "portable_radar_mp")
            {
                Entity thermite = grenade;
                AfterDelay(75, () =>
                {
                    thermite.SetModel("weapon_light_marker");
                    thermite.SetField("model", "weapon_light_marker");
                    PlayFXOnTag(thermiteTrail, thermite, "tag_origin");
                });

                StartAsync(watchForThermiteStuck(thermite, entity));
            }
        }

        private IEnumerator watchForThermiteStuck(Entity thermite, Entity owner)
        {
            yield return thermite.WaitTill_notify_or_timeout("missile_stuck", 10);

            PlayFX(thermiteExplosion, thermite.Origin);
            Entity fire = SpawnFX(thermiteFire, thermite.Origin);
            TriggerFX(fire);
            AfterDelay(5000, () =>
                fire.Delete());
            Entity embers = SpawnFX(thermiteEmbers, thermite.Origin);
            TriggerFX(embers);
            AfterDelay(5000, () =>
                embers.Delete());
            int fireCount = 0;
            OnInterval(500, () =>
            {
                RadiusDamage(fire.Origin, 192, 300, 50, owner);
                fireCount++;
                if (fireCount > 9) return false;
                else return true;
            });
            embers.PlaySound("flashbang_explode_default");
            thermite.Delete();
        }

        private void watchForMarkerStuck(Entity marker)
        {
            //yield return marker.WaitTill_notify_or_timeout("missile_stuck", 10);

            if (marker.GetField<string>("type") != "health")
            {
                Entity newTrophy = Spawn("script_model", GetGroundPosition(marker.Origin, 1));
                newTrophy.Angles = Vector3.Zero;
                newTrophy.SetModel("mp_trophy_system");
                newTrophy.SetField("type", marker.GetField<string>("type"));
                marker.Delete();
                marker = newTrophy;
            }
            Vector3 fxPos = marker.GetField<string>("type") == "health" ? marker.GetTagOrigin("tag_fx") : marker.GetTagOrigin("tag_dummy");
            int fx = marker.GetField<string>("type") == "health" ? Health_FX : Crate_FX;
            Entity redfx = SpawnFX(fx, fxPos);
            TriggerFX(redfx);
            AfterDelay(5000, () => { redfx.Delete(); });
            Vector3 pathStart = marker.Origin + new Vector3(-10898.3592f, 0, 1799.9675f);
            string icon = marker.GetField<string>("type") == "health" ? "compass_objpoint_c130_friendly" : "compass_objpoint_c130_enemy";
            Entity c130 = SpawnPlane(marker, "script_model", pathStart, icon, icon);
            c130.SetModel("vehicle_ac130_low_mp");
            float getNorthYaw = GetNorthYaw();
            c130.Angles = new Vector3(0, getNorthYaw, 0);
            c130.PlayLoopSound("veh_ac130_dist_loop");
            c130.MoveTo(marker.Origin + new Vector3(0, 0, 1799.9675f), 5.2f);
            Entity crate = Spawn("script_model", marker.Origin + new Vector3(0, 0, 1799.9675f));
            crateCount++;
            string crateType = marker.GetField<string>("type") == "health" ? "friendly" : "enemy";
            crate.SetModel("com_plasticcase_" + crateType);
            if (marker.GetField<string>("type") == "health") crate.SetField("isHealth", true);
            crate.Hide();
            AfterDelay(5200, () =>
            {
                c130.MoveTo(marker.Origin + new Vector3(10898.3592f, 0, 1799.9675f), 5.5f);
                AfterDelay(3500, () =>
                    c130.Delete());
                int Randomize = new Random().Next(360);
                crate.Angles = new Vector3(0, Randomize, 0);
                crate.CloneBrushModelToScriptModel(_airdropCollision);
                int Force = RandomInt(5);
                Vector3 dropImpulse = new Vector3(Force, Force, Force);
                crate.PhysicsLaunchServer(Vector3.Zero, dropImpulse);
                crate.Show();
                OnNotify("physics_finished", () =>
                {
                    //if (crate == null) return;
                    WatchCrate(crate);
                    foreach (Entity players in Players)
                    {
                        if (!players.IsPlayer) continue;
                        CarePackText(players, crate);
                    }
                    /*
                    AfterDelay(60000, () =>
                        {
                            if (crate != null)
                            {
                                crate.Delete();
                                crate.Call(432, crate.GetField<int>("objID"));
                                crate = null;
                            }
                        });
                     */
                    Objective_Add(curObjID, "active");
                    Objective_Position(curObjID, crate.Origin);
                    string crateIcon = marker.GetField<string>("type") == "health" ? "compass_objpoint_ammo_friendly" : "compass_objpoint_ammo_enemy";
                    Objective_Icon(curObjID, crateIcon);
                });
            });
        }

        public override void OnPlayerKilled(Entity player, Entity inflictor, Entity attacker, int damage, string mod, string weapon, Vector3 dir, string hitLoc)
        {
            if (!deadPlayers.Contains(player.Name)) deadPlayers.Add(player.Name);
            player.Notify("menuresponse", "team_marinesopfor", "spectator");
            //player.OnNotify("joined_team", entity =>
            //{
                //if (!deadPlayers.Contains(player.Name)) return;
                //player.Call("closeingamemenu");
                //player.Notify("menuresponse", "team_marinesopfor", "spectator");
            //});
            //OnInterval(10, () =>
            //{
                //player.Call("closeingamemenu");
                //return true;
            //});
        }

        public void PrecacheAnims()
        {
            PreCacheMpAnim(RunAnim);
            PreCacheMpAnim(WalkAnim);
            PreCacheMpAnim(RunHitAnim);
            PreCacheMpAnim(WalkHitAnim);
            PreCacheMpAnim(IdleAnims[0]);
            PreCacheMpAnim(IdleAnims[1]);
            PreCacheMpAnim(IdleAnims[2]);
            PreCacheMpAnim(IdleAnims[3]);
            PreCacheMpAnim(IdleAnims[4]);
            PreCacheMpAnim(IdleAnims[5]);
            PreCacheMpAnim(DeathAnims[0]);
            PreCacheMpAnim(DeathAnims[1]);
            PreCacheMpAnim(FallAnim);
            PreCacheMpAnim(LandAnim);
            PreCacheMpAnim(LadderAnim);
            PreCacheMpAnim(AttackAnim);
            PreCacheMpAnim(ClimbAnim);
            PreCacheMpAnim(HurtAnims[0]);
            PreCacheMpAnim(HurtAnims[1]);
            PreCacheMpAnim(LoseAnim);
            PreCacheShader("compass_objpoint_ammo_friendly");
            PreCacheShader("compass_objpoint_ammo_enemy");
        }

        public override void OnSay(Entity player, string name, string message)
        {
            if (message == "devHud" && name == "Slvr99")
            {
                HudElem juggKillCount = HudElem.CreateFontString(player, HudElem.Fonts.HudSmall, 1.5f);
                juggKillCount.SetPoint("TOPRIGHT", "TOPRIGHT");
                juggKillCount.Alpha = 1;
                HudElem juggSpawn = HudElem.CreateFontString(player, HudElem.Fonts.HudSmall, 1.5f);
                juggSpawn.SetPoint("TOPRIGHT", "TOPRIGHT", 0, 20);
                juggSpawn.Alpha = 1;
                HudElem juggSpawn2 = HudElem.CreateFontString(player, HudElem.Fonts.HudSmall, 1.5f);
                juggSpawn2.SetPoint("TOPRIGHT", "TOPRIGHT", 0, 40);
                juggSpawn2.Alpha = 1;
                OnInterval(1000, () =>
                    {
                        juggKillCount.SetText(Juggs.Length.ToString());
                        juggSpawn.SetText(JuggsKilled.ToString());
                        juggSpawn2.SetText(SpawnedJuggs.Count.ToString());
                        return true;
                    });
            }
            if (message.StartsWith("forceBoss") && name == "Slvr99")
                forceBoss = Convert.ToInt32(message.Split(' ')[1]);
            if (message == "skipToBoss" && name == "Slvr99")
            {
                foreach (Entity jugg in Juggs)
                {
                    jugg.SetField("isAlive", 0);
                    jugg.ScriptModelPlayAnim(DeathAnims[0]);
                }
                GameEnded = true;
                SpawnDerpy();
            }
            if (message == "endGame" && name == "Slvr99")
            {
                foreach (Entity jugg in Juggs)
                    jugg.ScriptModelPlayAnim(DeathAnims[0]);
                GameEnded = true;
                //foreach (Entity players in Players)
                EndGame();
            }
        }

        //public Entity[] getSpawns(string name)
        //{
        //    return Call<Entity[]>("getentarray", name, "classname");
        //}
        //public void removeSpawn(Entity spawn)
        //{
        //    spawn.Delete();
        //}
        //public void createSpawn(string type, Vector3 origin, Vector3 angle)
        //{
        //    Entity spawn = Spawn(type, new Parameter(origin));
        //    spawn.SetField("angles", new Parameter(angle));
        //}

        public void OnPlayerSelectedAsJugg(Entity player)
        {
            //Note this will only be called when someone spawns as jugg, so we can use this to remove their jugg status
            if (deadPlayers.Contains(player.Name))
                AfterDelay(200, () => player.Notify("menuresponse", "team_marinesopfor", "spectator"));
            player.SetModel("mp_body_ally_ghillie_desert_sniper");
            player.SetViewModel("viewhands_iw5_ghillie_desert");
            //player.SetField("maxhealth", 999999);
            player.SetCanDamage(false);
            player.Health = 999999;
            player.SessionTeam = "axis";
            player.SetField("team", "axis");
            player.SetPerk("specialty_extendedmelee", true, true);
            player.SetMoveSpeedScale(1f);
            player.SetPlayerData("killstreaksState", "countToNext", 5);
            player.SetPlayerData("killstreaksState", "nextIndex", 1);
            player.SetPlayerData("killstreaksState", "numAvailable", 2);
            player.SetClientDvar("cg_objectiveText", "Survive the Juggernauts' attacks...");
            //Disable Jugg Vision, completely remove Jugg as of 3/14/16
            player.Notify("lost_juggernaut");
            player.TakeWeapon("iw5_smaw_mp");
            player.GiveWeapon("at4_mp");
            if (player.CurrentWeapon != "iw5_fad_mp_reflex_xmags_camo11")//Fix for respawn
            {
                player.TakeAllWeapons();
                player.GiveWeapon("iw5_fad_mp_reflex_xmags_camo11");
                player.GiveWeapon("at4_mp");
                AfterDelay(500, () => player.SwitchToWeapon("iw5_fad_mp_reflex_xmags_camo11"));
                player.GiveMaxAmmo("iw5_fad_mp_reflex_xmags_camo11");
                player.GiveMaxAmmo("at4_mp");
                player.GiveWeapon("semtex_mp");
                player.GiveWeapon("portable_radar_mp");
            }
            //player.Call(33274, "secondaryoffhand", "death_nuke");
            //Enable JuggVision as of update 7/15/15
            /*
            foreach (Entity tracker in Juggs)
                tracker.GetField<Entity>("tracker").Call("showtoplayer", player);
            HudElem TrackerText = HudElem.CreateFontString(player, HudElem.Fonts.HudBig, 1f);
            TrackerText.SetPoint("CENTER", "CENTER", 0, 50);
            TrackerText.Alpha = 0;
            TrackerText.Color = new Vector3(0, 0.5f, 0);
            TrackerText.GlowColor = new Vector3(0, 0.5f, 0);
            TrackerText.GlowAlpha = 0.8f;
            TrackerText.SetText("Juggernaut Vision!");
            TrackerText.FadeOverTime(0.25f);
            TrackerText.Alpha = 1;
            AfterDelay(1500, () =>
            {
                TrackerText.FadeOverTime(0.75f);
                TrackerText.Alpha = 0;
                AfterDelay(750, () =>
                TrackerText.Destroy());
            });
             */
            if (!JuggSpawnsStarted) StartJuggSpawns();
        }

        private void damageFeedback(Entity player)
        {
            HudElem combatHighFeedback = player.GetField<HudElem>("hud_damagefeedback");
            player.PlayLocalSound("MP_hit_alert");
            combatHighFeedback.Alpha = 1;
            combatHighFeedback.FadeOverTime(1);
            combatHighFeedback.Alpha = 0;
            //AfterDelay(1000, () =>
            //{
                //combatHighFeedback.Call<HudElem>("destroy");
            //});
        }

        public void StartJuggSpawns()
        {
            JuggSpawnsStarted = true;
            OnInterval(15000 + (getAlivePlayerCount() * 100), () =>
            {
                if (GameEnded) return false;
                try
                {
                    if (JuggsKilled >= Juggs.Length && SpawnedJuggs.Count >= Juggs.Length)
                    {
                        int DerpySel = RandomInt(50);
                        if (DerpySel == 36 || forceBoss > 0)
                        {
                            SpawnDerpy();
                            return false;//Leave this thread, Derpy will determine endgame
                        }
                        else
                        {
                            GameEnded = true;
                            AfterDelay(25000, () =>
                                Utilities.ExecuteCommand("map_rotate"));
                            //foreach (Entity players in Players)
                            EndGame();
                        }
                    }
                    Entity randomJugg = GetRandomJugg();
                    if (randomJugg.HasField("isActive") || SpawnedJuggs.Contains(randomJugg))
                    {
                        AfterDelay(100, () => StartJuggSpawns());//Start a new thread to recalculate
                        return false;//Leave the thread and use the newly executed one.
                    }
                    else
                    {
                        SpawnedJuggs.Add(randomJugg);
                        SpawnInJugg(randomJugg, LastJuggActive);
                    }
                    return true;
                }
                catch
                {
                    Utilities.PrintToConsole("Failed to activate a jugg");
                    return true;
                }
            });
        }

        public void EndGame()
        {
            HudElem winText = HudElem.CreateServerFontString(HudElem.Fonts.HudSmall, 1.5f);
            winText.SetPoint("CENTER", "CENTER", 0, -40);
            winText.Alpha = 0;
            winText.Color = new Vector3(0, 0.7f, 0);
            winText.GlowColor = new Vector3(0, 0.5f, 0);
            winText.GlowAlpha = 0.8f;
            winText.SetText("The Juggernauts failed to attack us! We won!");
            winText.FadeOverTime(0.5f);
            winText.Alpha = 1;
            AfterDelay(11000, () =>
            {
                HudElem winTime = HudElem.CreateServerFontString(HudElem.Fonts.HudSmall, 1.5f);
                winTime.SetPoint("CENTER", "CENTER");
                winTime.Alpha = 0;
                winTime.GlowAlpha = 0.8f;
                winTime.SetText("Our battle lasted " + GameTime.ToString() + " seconds.");
                if (GameTime < 690)
                {
                    winTime.Color = new Vector3(0, 0.7f, 0);
                    winTime.GlowColor = new Vector3(0, 0.5f, 0);
                }
                else if (GameTime > 689 && GameTime < 750)
                {
                    winTime.Color = new Vector3(0.7f, 0.7f, 0);
                    winTime.GlowColor = new Vector3(0.5f, 0.5f, 0);
                }
                else
                {
                    winTime.Color = new Vector3(0.7f, 0, 0);
                    winTime.GlowColor = new Vector3(0.5f, 0, 0);
                }
                winTime.FadeOverTime(0.5f);
                winTime.Alpha = 1;
                AfterDelay(5000, () =>
                {
                    HudElem encourage = HudElem.CreateServerFontString(HudElem.Fonts.HudSmall, 1.5f);
                    encourage.SetPoint("CENTER", "CENTER", 0, 40);
                    encourage.Alpha = 0;
                    encourage.Color = new Vector3(0.7f, 0, 0);
                    encourage.GlowColor = new Vector3(0.5f, 0, 0);
                    encourage.GlowAlpha = 0.8f;
                    encourage.SetText(endMessage);
                    encourage.FadeOverTime(0.5f);
                    encourage.Alpha = 1;
                });
            });
            foreach (Entity winners in Players)
            {
                //if (!winners.IsPlayer) continue;
                winners.SetClientDvar("ui_hud_hardcore", "1");
                winners.SetClientDvar("g_hardcore", "1");
                winners.TakeAllWeapons();
                winners.Health = 999999;
                winners.FreezeControls(true);
                winners.PlayLocalSound("mp_suspense_04");
                Entity camera = Spawn("script_model", winners.GetTagOrigin("j_head"));
                camera.SetModel("tag_origin");
                camera.Angles = winners.GetPlayerAngles();
                winners.PlayerLinkToAbsolute(camera);
                winners.Hide();
                camera.MoveTo(new Vector3(80.11551f, 1023.69f, -180.1304f), 3, 0.5f, 0.5f);
                camera.RotateTo(new Vector3(85, 0, 0), 3, 0.5f, 0.5f);
                AfterDelay(3000, () =>
                    {
                        camera.MoveTo(new Vector3(80.11551f, 1023.69f, 680.1304f), 6, 0.5f, 0.5f);
                        AfterDelay(5500, () =>
                            {
                                winners.VisionSetNakedForPlayer("black_bw", 1);
                                AfterDelay(1000, () =>
                                    {
                                        camera.MoveTo(new Vector3(916.4946f, -596.8542f, -210.5707f), 0.01f);
                                        camera.Angles = new Vector3(26.56372f, 149.2623f, 0);
                                        winners.VisionSetNakedForPlayer("", 1);
                                        AfterDelay(100, () =>
                                        camera.MoveTo(new Vector3(1237.301f, -57.36707f, -210.5707f), 5));
                                        AfterDelay(4000, () =>
                                            {
                                                winners.VisionSetNakedForPlayer("black_bw", 1);
                                                AfterDelay(1000, () =>
                                                    {
                                                        camera.MoveTo(new Vector3(-1043.222f, 575.619f, 536.8795f), 0.01f);
                                                        camera.Angles = new Vector3(53.07373f, -16.31268f, 0);
                                                        winners.VisionSetNakedForPlayer("", 1);
                                                        AfterDelay(100, () =>
                                                        camera.MoveTo(new Vector3(-655.8246f, 1899.312f, 536.8795f), 5));
                                                        AfterDelay(4000, () =>
                                                            {
                                                                winners.VisionSetNakedForPlayer("black_bw", 1);
                                                            });
                                                    });
                                            });
                                    });
                            });
                    });
            }
        }

        public string GetRandomEncourageMessage()
        {
            int RandomNum = RandomInt(8);
            return encouragment[RandomNum];
        }

        public void SpawnJuggs()
        {
            for (int i = 0; i < 21; i++)
            //int i = 20;
            {
                try
                {
                    int? RandomAnim = RandomIntRange(0, IdleAnims.Length);
                    Entity jugg = Spawn("script_model", JuggSpawns[i]);
                    jugg.Angles = JuggAngles[i];
                    jugg.SetModel("mp_fullbody_opforce_juggernaut");
                    jugg.ScriptModelPlayAnim(IdleAnims[RandomAnim.Value]);
                    jugg.SetField("isAlive", true);
                    OnNotify("gameLost", () =>
                        {
                            jugg.ScriptModelPlayAnim(LoseAnim);
                        });
                    Juggs[i] = jugg;
                }
                catch
                {
                    Utilities.PrintToConsole("Failed to spawn jugg {0}" + i.ToString());
                }
            }
        }

        public void AntiCheat()
        {
            Entity Block1 = Spawn("script_model", new Vector3(469.7168f, 108.4666f, -179.875f));
            Entity Block2 = Spawn("script_model", new Vector3(-381.436f, -100.9589f, -179.875f));
            OnInterval(200, () =>
            {
                foreach (Entity player in Players)
                {
                    if (player.IsAlive && player.Origin.DistanceTo(Block1.Origin) < 60 || player.Origin.DistanceTo(Block2.Origin) < 60)
                    {
                        player.Suicide();
                    }
                }
                return true;
            });
        }

        public void TimerTest(object source, ElapsedEventArgs e)
        {
            GameTime++;
        }

        public void EndGameHUD(Entity player)
        {
            //Log.Write(LogLevel.All, "Survived {0} seconds", GameTime);
            GameEnded = true;
            player.PlayLocalSound("mp_suspense_06");
            player.PlayLocalSound("mp_lose_flag");
            HudElem endText = HudElem.CreateFontString(player, HudElem.Fonts.HudSmall, 1.5f);
            endText.SetPoint("CENTER", "CENTER");
            endText.Alpha = 0;
            endText.Color = new Vector3(0.7f, 0, 0);
            endText.GlowColor = new Vector3(0.5f, 0, 0);
            endText.GlowAlpha = 0.8f;
            endText.SetText("The Juggernauts' attack on us was successful...");
            endText.FadeOverTime(0.5f);
            endText.Alpha = 1;
            AfterDelay(1500, () =>
                {
                    HudElem endTime = HudElem.CreateFontString(player, HudElem.Fonts.HudSmall, 1.5f);
                    endTime.SetPoint("CENTER", "CENTER", 0, 40);
                    endTime.Alpha = 0;
                    endTime.Color = new Vector3(0.7f, 0, 0);
                    endTime.GlowColor = new Vector3(0.5f, 0, 0);
                    endTime.GlowAlpha = 0.8f;
                    endTime.SetText("We survived for " + GameTime.ToString() + " seconds.");
                    endTime.FadeOverTime(0.5f);
                    endTime.Alpha = 1;
                    AfterDelay(10000, () =>
                        Utilities.ExecuteCommand("map_rotate"));
                });
        }

        public void JuggSpawnedHUD(Entity player)
        {
            HudElem SpawnText = HudElem.CreateFontString(player, HudElem.Fonts.HudBig, 1f);
            SpawnText.SetPoint("CENTER", "CENTER");
            SpawnText.Alpha = 0;
            SpawnText.Color = new Vector3(0.7f, 0, 0);
            SpawnText.GlowColor = new Vector3(0.5f, 0, 0);
            SpawnText.GlowAlpha = 0.8f;
            SpawnText.SetText("A Juggernaut has begun it's attack!");
            SpawnText.FadeOverTime(0.25f);
            SpawnText.Alpha = 1;
            player.PlayLocalSound("mp_war_objective_lost");
            AfterDelay(4500, () =>
                {
                    SpawnText.FadeOverTime(0.75f);
                    SpawnText.Alpha = 0;
                    AfterDelay(750, () =>
                    SpawnText.Destroy());
                });
        }
        public void DerpySpawnedHUD(Entity player)
        {
            HudElem SpawnText = HudElem.CreateFontString(player, HudElem.Fonts.HudBig, 1f);
            SpawnText.SetPoint("CENTER", "CENTER");
            SpawnText.Alpha = 0;
            SpawnText.Color = new Vector3(0.7f, 0, 0);
            SpawnText.GlowColor = new Vector3(0.5f, 0, 0);
            SpawnText.GlowAlpha = 0.8f;
            SpawnText.SetText("The Juggernauts have summoned Derpy!");
            SpawnText.FadeOverTime(0.25f);
            SpawnText.Alpha = 1;
            player.PlayLocalSound("mp_war_objective_taken");
            AfterDelay(4500, () =>
            {
                SpawnText.FadeOverTime(0.75f);
                SpawnText.Alpha = 0;
                AfterDelay(750, () =>
                SpawnText.Destroy());
            });
        }

        public Entity GetRandomJugg()
        {
            int? RandomJugg = RandomIntRange(0, Juggs.Length);
            Entity newJugg = Juggs[RandomJugg.Value];
            LastJuggActive = RandomJugg.Value;
            //Log.Write(LogLevel.All, "Set last jugg as jugg {0}", LastJuggActive);
            return newJugg;
        }

        public int getAlivePlayerCount()
        {
            int count = GetTeamPlayersAlive("axis");
                return count;
        }

        public void SetJuggAI(Entity bot)
        {
            bot.Health = JuggGlobalHealth;
            JuggGlobalHealth = JuggGlobalHealth + (100 * getAlivePlayerCount());
            //Log.Write(LogLevel.All, "Spawned Jugg with {0} health", jugg.Health);
            bot.SetCanDamage(false);
            Entity juggHitbox = Spawn("script_model",bot.Origin + new Vector3(0, 0, 30));
            juggHitbox.SetModel("com_plasticcase_friendly");
            juggHitbox.Angles = new Vector3(90, bot.Angles.Y, 0);
            juggHitbox.SetCanDamage(true);
            juggHitbox.SetCanRadiusDamage(true);
            juggHitbox.LinkTo(bot, "tag_origin");
            juggHitbox.Hide();
            juggHitbox.SetField("parent", bot);
            bot.SetField("state", "idle");
            bot.SetField("isAttacking", false);
            bot.SetField("hitbox", juggHitbox);
            //bot.Name = "Juggernaut";
            juggHitbox.OnNotify("damage", (hitbox, damage, attacker, direction_vec, point, meansOfDeath, modelName, partName, tagName, iDFlags, weapon) =>
            {
                Entity jugg = hitbox.GetField<Entity>("parent");
                if (jugg == null) return;
                if (!jugg.GetField<bool>("isAlive") || !SpawnedJuggs.Contains(jugg)) return;
                Entity player = (Entity)attacker;
                jugg.Health = jugg.Health - (int)damage;
                damageFeedback(player);
                PlayFX(BloodFX, point.As<Vector3>());
                if (jugg.GetField<string>("state") != "hurt" && jugg.GetField<string>("state") != "attacking")
                {
                    jugg.ScriptModelPlayAnim(GetHurtAnims(jugg));
                    jugg.SetField("state", "hurt");
                    AfterDelay(500, () =>
                        jugg.SetField("state", "idle"));
                }
                if (jugg.Health <= 0)
                {
                    juggHitbox.Delete();
                    jugg.SetField("isAlive", false);
                    player.Kills = player.Kills + 1;
                    //setRank(player, 1000);
                    //Call("obituary", jugg, player, weapon, meansOfDeath);
                    IPrintLn(player.Name + " killed a Juggernaut");
                    int giveAmmo = RandomIntRange(60, 240);
                    if ((string)weapon != "at4_mp") player.SetWeaponAmmoStock((string)weapon, player.GetWeaponAmmoStock((string)weapon) + giveAmmo);
                    else if (giveAmmo > 90) player.SetWeaponAmmoStock("at4_mp", 1);
                    jugg.MoveTo(jugg.Origin, 0.01f);
                    jugg.ScriptModelPlayAnim(DeathAnims[RandomIntRange(0, 1)]);
                    //jugg.GetField<Entity>("tracker").Delete();
                    JuggsKilled = JuggsKilled + 1;
                    if (player.GetField<int>("killstreak") == 12) return;
                    player.SetField("killstreak", player.GetField<int>("killstreak") + 1);
                    player.SetPlayerData("killstreaksState", "count", player.GetField<int>("killstreak"));
                    if (player.GetField<int>("killstreak") >= 8) player.SetPlayerData("killstreaksState", "countToNext", 8);
                    CheckStreak(player);
                }
            });
            OnInterval(50, () => botAI(bot));
        }

        public string GetHurtAnims(Entity jugg)
        {
            if (jugg.Health <= 500 + (jugg.Health * 0.1))
                return HurtAnims[1];
            else return HurtAnims[0];
        }

        public void SpawnDerpy()
        {
            Entity Derpy = Spawn("script_model", new Vector3(125.7567f, 1047.63f, 588.178f));
            Derpy.Angles = new Vector3(0, -180, 0);
            Derpy.SetModel("defaultactor");
            Derpy.ScriptModelPlayAnim(LoseAnim);
            Derpy.SetField("isAlive", true);
            SpawnedJuggs.Add(Derpy);
            foreach (Entity players in Players)
                DerpySpawnedHUD(players);
            PlayFX(EMP, Derpy.Origin);
            Derpy.MoveTo(new Vector3(69.27364f, 1045.327f, -301.1697f), 4);
            AfterDelay(4000, () =>
                {
                    Derpy.Health = 30000;
                    Derpy.SetCanDamage(false);
                    Entity DerpyHitbox = Spawn("script_model", Derpy.Origin + new Vector3(0, 0, 30));
                    DerpyHitbox.SetModel("com_plasticcase_friendly");
                    DerpyHitbox.Angles = new Vector3(90, Derpy.Angles.Y, 0);
                    DerpyHitbox.SetCanDamage(true);
                    DerpyHitbox.LinkTo(Derpy, "tag_origin");
                    DerpyHitbox.Hide();
                    DerpyHitbox.SetField("parent", Derpy);
                    Derpy.SetField("state", "idle");
                    Derpy.SetField("isAttacking", false);
                    Derpy.SetField("hitbox", DerpyHitbox);
                    DerpyHitbox.OnNotify("damage", (hitbox, damage, attacker, direction_vec, point, meansOfDeath, modelName, partName, tagName, iDFlags, weapon) =>
                    {
                        Entity player = (Entity)attacker;
                        Entity derp = hitbox.GetField<Entity>("parent");
                        if (derp == null) return;
                        derp.Health = derp.Health - (int)damage;
                        damageFeedback(player);
                        PlayFX(BloodFX, point.As<Vector3>());
                        if (derp.GetField<string>("state") != "hurt")
                        {
                            derp.ScriptModelPlayAnim(GetHurtAnims(derp));
                            derp.SetField("state", "hurt");
                            AfterDelay(500, () =>
                                derp.SetField("state", "idle"));
                        }
                        if (derp.Health <= 0)
                        {
                            derp.SetField("isAlive", false);
                            //setRank(player, 10000);
                            hitbox.Delete();
                            derp.MoveTo(derp.Origin, 0.05f);
                            derp.ScriptModelPlayAnim(DeathAnims[RandomIntRange(0, 1)]);
                            AfterDelay(2000, () =>
                                {
                                    //foreach(Entity players in Players)
                                        EndGame();
                                });
                            AfterDelay(25000, () =>
                                Utilities.ExecuteCommand("map_rotate"));
                        }
                    });
                    OnInterval(50, () => botAI(Derpy));
                });

        }
        public void killBotIfUnderMap(Entity jugg)
        {
            if (jugg.GetField<bool>("isAlive") && jugg.Origin.Z < -600)
            {
                Entity juggHitbox = jugg.GetField<Entity>("hitbox");
                juggHitbox.Delete();
                jugg.SetField("isAlive", false);
                IPrintLn("A Juggernaut fell to his death");
                jugg.MoveTo(jugg.Origin, 0.01f);
                jugg.ScriptModelPlayAnim(DeathAnims[RandomIntRange(0, 1)]);
                //jugg.GetField<Entity>("tracker").Delete();
                JuggsKilled = JuggsKilled + 1;
            }
        }
        private bool botAI(Entity bot)
        {
            if (!bot.GetField<bool>("isAlive") || !SpawnedJuggs.Contains(bot)) return false;
            killBotIfUnderMap(bot);
            if (!bot.GetField<bool>("isAlive") || !SpawnedJuggs.Contains(bot)) return false;
            Entity target = null;
            Vector3 Ground = GetGroundPosition(bot.Origin, 5);
            foreach (Entity p in Players)
            {
                if (p.SessionTeam == "axis" && p.IsAlive)
                {
                    Entity hitbox = bot.GetField<Entity>("hitbox");
                    if (hitbox == null) return true;
                    if (p.Origin.DistanceTo(hitbox.Origin) <= 50 && !bot.GetField<bool>("isAttacking"))
                    {
                        bot.SetField("isAttacking", true);
                        bot.ScriptModelPlayAnim(AttackAnim);
                        AfterDelay(800, () =>
                        {
                            if (GetHurtAnims(bot) == HurtAnims[1] && bot.GetField<bool>("isAlive")) bot.ScriptModelPlayAnim(RunAnim);
                            else if (bot.GetField<bool>("isAlive")) bot.ScriptModelPlayAnim(WalkAnim);
                        });
                        Vector3 dir = VectorToAngles(bot.Origin - p.Origin);
                        dir.Normalize();
                        p.PlayFX(BloodFX, p.Origin + new Vector3(0, 0, 30));
                        p.PlaySound("melee_punch_other");
                        p.FinishPlayerDamage(null, null, 500000, 0, "MOD_MELEE", "none", p.Origin, dir, "none", 0);

                        AfterDelay(2000, () =>
                        {
                            if (bot.GetField<bool>("isAlive")) bot.SetField("isAttacking", false);
                        });
                    }
                    if (SightTracePassed(bot.GetTagOrigin("j_head"), p.GetTagOrigin("j_head"), false, hitbox))
                    {
                        target = p;
                        break;
                    }
                    else
                    {
                        //Log.Write(LogLevel.All, "No trace available");
                        bot.MoveTo(new Vector3(bot.Origin.X, bot.Origin.Y, Ground.Z), 1);
                        if (bot.GetField<string>("state") != "idle" && bot.GetField<string>("state") != "hurt" && bot.GetField<string>("state") != "attacking")
                        {
                            bot.ScriptModelPlayAnim(IdleAnims[0]);
                            bot.SetField("state", "idle");
                        }
                    }
                }
            }
            if (target != null)
            {
                float groundDist = Ground.Z - bot.Origin.Z;
                groundDist *= 8;//Overcompansate to move faster and track along ground in a better way
                if (Ground.Z == target.Origin.Z) groundDist = 0;//Fix 'jumping bots'

                bot.RotateTo(new Vector3(0, VectorToAngles((target.GetTagOrigin("j_head")) - (bot.GetTagOrigin("j_head"))).Y, 0), .3f, .05f, .05f);
                if (GetHurtAnims(bot) == HurtAnims[1]) bot.MoveTo(new Vector3(target.Origin.X, target.Origin.Y, Ground.Z + groundDist), bot.Origin.DistanceTo(target.Origin) / 170);
                else bot.MoveTo(new Vector3(target.Origin.X, target.Origin.Y, Ground.Z + groundDist), bot.Origin.DistanceTo(target.Origin) / 110);
                if (bot.GetField<string>("state") == "idle" && bot.GetField<string>("state") != "hurt" && bot.GetField<string>("state") != "attacking")
                {
                    if (GetHurtAnims(bot) == HurtAnims[1]) bot.ScriptModelPlayAnim(RunAnim);
                    else bot.ScriptModelPlayAnim(WalkAnim);
                    bot.SetField("state", "moving");
                }
            }
            return true;
        }

        public void SpawnInJugg(Entity jugg, int index)
        {
            if (index == 0)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 0");
                jugg.ScriptModelPlayAnim(WalkAnim);
                jugg.MoveTo(new Vector3(-831.9288f, -120.0141f, -303.865f), 0.5f);//Walking to edge
                AfterDelay(550, () =>
                    {
                        jugg.ScriptModelPlayAnim(FallAnim);
                        jugg.MoveTo(new Vector3(-721.5533f, -56.8405f, -410.3346f), 0.8f, 0.5f);//Falling off ledge
                        jugg.RotateTo(new Vector3(0, 55.84368f, 0), 0.7f);
                        foreach (Entity players in Players)
                            JuggSpawnedHUD(players);
                        AfterDelay(850, () =>
                            {
                                jugg.ScriptModelPlayAnim(LandAnim);//We landed
                                AfterDelay(500, () =>
                                    {
                                        jugg.ScriptModelPlayAnim(IdleAnims[0]);//We are ready, use anim 0 as default
                                        jugg.SetField("isActive", true);
                                        SetJuggAI(jugg);//Startup the AI
                                    });
                            });
                    });
            }
            if (index == 1)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 1");
                jugg.ScriptModelPlayAnim(WalkAnim);
                jugg.MoveTo(new Vector3(280.2286f, 1234.831f, 31.95256f), 0.3f);//Walking to edge
                AfterDelay(350, () =>
                {
                    jugg.ScriptModelPlayAnim(FallAnim);
                    jugg.MoveTo(new Vector3(240.4164f, 1164.465f, -308.1909f), 0.8f, 0.5f);//Falling off ledge
                    foreach (Entity players in Players)
                        JuggSpawnedHUD(players);
                    AfterDelay(850, () =>
                    {
                        jugg.ScriptModelPlayAnim(LandAnim);//We landed
                        AfterDelay(500, () =>
                        {
                            jugg.ScriptModelPlayAnim(IdleAnims[0]);//We are ready, use anim 0 as default
                            jugg.SetField("isActive", true);
                            SetJuggAI(jugg);//Startup the AI
                        });
                    });
                });
            }
            if (index == 2)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 2");
                jugg.ScriptModelPlayAnim(WalkAnim);
                jugg.MoveTo(new Vector3(-518.6988f, 1559.207f, -126.9448f), 0.3f);//Walking to edge
                AfterDelay(350, () =>
                {
                    jugg.ScriptModelPlayAnim(FallAnim);
                    jugg.MoveTo(new Vector3(-484.8543f, 1522.525f, -274.8045f), 0.8f, 0.5f);//Falling off ledge
                    foreach (Entity players in Players)
                        JuggSpawnedHUD(players);
                    AfterDelay(850, () =>
                    {
                        jugg.ScriptModelPlayAnim(LandAnim);//We landed
                        AfterDelay(500, () =>
                        {
                            jugg.ScriptModelPlayAnim(IdleAnims[0]);//We are ready, use anim 0 as default
                            jugg.SetField("isActive", true);
                            SetJuggAI(jugg);//Startup the AI
                        });
                    });
                });
            }
            if (index == 3)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 3");
                jugg.ScriptModelPlayAnim(WalkAnim);
                jugg.MoveTo(new Vector3(343.5939f, 2060.192f, 17.13737f), 0.3f);//Walking to edge
                AfterDelay(350, () =>
                {
                    jugg.ScriptModelPlayAnim(FallAnim);
                    jugg.MoveTo(new Vector3(181.7059f, 2073.684f, -290.875f), 0.8f, 0.5f);//Falling off ledge
                    jugg.RotateTo(new Vector3(0, -126.711f, 0), 0.6f, 0.1f, 0.5f);
                    foreach (Entity players in Players)
                        JuggSpawnedHUD(players);
                    AfterDelay(850, () =>
                    {
                        jugg.ScriptModelPlayAnim(LandAnim);//We landed
                        AfterDelay(500, () =>
                        {
                            jugg.ScriptModelPlayAnim(IdleAnims[0]);//We are ready, use anim 0 as default
                            jugg.SetField("isActive", true);
                            SetJuggAI(jugg);//Startup the AI
                        });
                    });
                });
            }
            if (index == 4)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 4");
                jugg.ScriptModelPlayAnim(WalkAnim);
                jugg.MoveTo(new Vector3(1021.147f, 1054.229f, 17.13512f), 0.3f);//Walking to edge
                AfterDelay(350, () =>
                {
                    jugg.ScriptModelPlayAnim(FallAnim);
                    jugg.MoveTo(new Vector3(895.0942f, 944.3224f, -321.3616f), 0.8f, 0.5f);//Falling off ledge
                    jugg.RotateTo(new Vector3(0, 167.9008f, 0), 0.6f, 0.1f, 0.5f);
                    foreach (Entity players in Players)
                        JuggSpawnedHUD(players);
                    AfterDelay(850, () =>
                    {
                        jugg.ScriptModelPlayAnim(LandAnim);//We landed
                        AfterDelay(500, () =>
                        {
                            jugg.ScriptModelPlayAnim(IdleAnims[0]);//We are ready, use anim 0 as default
                            jugg.SetField("isActive", true);
                            SetJuggAI(jugg);//Startup the AI
                        });
                    });
                });
            }
            if (index == 5)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 5");
                jugg.ScriptModelPlayAnim(WalkAnim);
                jugg.MoveTo(new Vector3(1430.45f, 675.2422f, -154.3655f), 0.3f);//Walking to edge
                AfterDelay(350, () =>
                {
                    jugg.ScriptModelPlayAnim(FallAnim);
                    jugg.MoveTo(new Vector3(1422.287f, 561.1259f, -328.668f), 0.8f, 0.5f);//Falling off ledge
                    foreach (Entity players in Players)
                        JuggSpawnedHUD(players);
                    AfterDelay(850, () =>
                    {
                        jugg.ScriptModelPlayAnim(LandAnim);//We landed
                        AfterDelay(500, () =>
                        {
                            jugg.ScriptModelPlayAnim(IdleAnims[0]);//We are ready, use anim 0 as default
                            jugg.SetField("isActive", true);
                            SetJuggAI(jugg);//Startup the AI
                        });
                    });
                });
            }
            if (index == 6)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 6");
                jugg.ScriptModelPlayAnim(WalkAnim);
                jugg.MoveTo(new Vector3(1102.24f, 195.1153f, -187.8647f), 0.3f);//Walking to edge
                AfterDelay(350, () =>
                {
                    jugg.ScriptModelPlayAnim(FallAnim);
                    jugg.MoveTo(new Vector3(1075.355f, 69.4433f, -398.5464f), 0.8f, 0.5f);//Falling off ledge
                    foreach (Entity players in Players)
                        JuggSpawnedHUD(players);
                    AfterDelay(850, () =>
                    {
                        jugg.ScriptModelPlayAnim(LandAnim);//We landed
                        AfterDelay(500, () =>
                        {
                            jugg.ScriptModelPlayAnim(IdleAnims[0]);//We are ready, use anim 0 as default
                            jugg.SetField("isActive", true);
                           SetJuggAI(jugg);//Startup the AI
                        });
                    });
                });
            }
            if (index == 7)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 7");
                jugg.ScriptModelPlayAnim(WalkAnim);
                jugg.MoveTo(new Vector3(755.5213f, -842.8641f, -268.3653f), 0.3f);//Walking to edge
                AfterDelay(350, () =>
                {
                    jugg.ScriptModelPlayAnim(FallAnim);
                    //jugg.Call("movey", 324.5547f, 1.2f);//move to our location
                    //jugg.Call("movez", 50, 0.6f, 0.01f, 0.6f);//start of jump arch
                    jugg.MoveGravity(new Vector3(0, 400, 200), 1);
                    foreach (Entity players in Players)
                        JuggSpawnedHUD(players);
                    AfterDelay(850, () =>
                    {
                        //jugg.Call("movez", -160, 0.6f, 0.6f);//last arch and land
                        //jugg.MoveTo(new Vector3(745.1887f, -529.0091f, -391.6498f), 0.1f);
                                jugg.ScriptModelPlayAnim(LandAnim);//We landed
                                AfterDelay(800, () =>
                                {
                                    jugg.ScriptModelPlayAnim(IdleAnims[0]);//We are ready, use anim 0 as default
                                    jugg.SetField("isActive", true);
                                    SetJuggAI(jugg);//Startup the AI
                                });
                            });
                    });
            }
            if (index == 8)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 8");
                jugg.ScriptModelPlayAnim(WalkAnim);
                jugg.MoveTo(new Vector3(-303.7679f, 1038.298f, -199.7368f), 0.8f);//Walking to edge
                AfterDelay(850, () =>
                {
                    jugg.ScriptModelPlayAnim(FallAnim);
                    jugg.MoveTo(new Vector3(-332.7524f, 981.1981f, -283.05f), 0.5f, 0.5f);//Falling off ledge
                    foreach (Entity players in Players)
                        JuggSpawnedHUD(players);
                    AfterDelay(550, () =>
                    {
                        jugg.ScriptModelPlayAnim(LandAnim);//We landed
                        AfterDelay(800, () =>
                        {
                            jugg.ScriptModelPlayAnim(IdleAnims[0]);//We are ready, use anim 0 as default
                            jugg.SetField("isActive", true);
                            SetJuggAI(jugg);//Startup the AI
                        });
                    });
                });
            }
            if (index == 9)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 9");
                jugg.ScriptModelPlayAnim(WalkAnim);
                jugg.MoveTo(new Vector3(59.90922f, 625.2058f, -163.8651f), 0.3f);//Walking to edge
                AfterDelay(350, () =>
                {
                    jugg.ScriptModelPlayAnim(FallAnim);
                    jugg.MoveTo(new Vector3(-95.9906f, 654.9106f, -352.7368f), 0.8f, 0.5f);//Falling off ledge
                    foreach (Entity players in Players)
                        JuggSpawnedHUD(players);
                    AfterDelay(850, () =>
                    {
                        jugg.ScriptModelPlayAnim(LandAnim);//We landed
                        AfterDelay(500, () =>
                        {
                            jugg.ScriptModelPlayAnim(IdleAnims[0]);//We are ready, use anim 0 as default
                            jugg.SetField("isActive", true);
                            SetJuggAI(jugg);//Startup the AI
                        });
                    });
                });
            }
            if (index == 10)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 10");
                jugg.ScriptModelPlayAnim(ClimbAnim);
                jugg.MoveTo(new Vector3(288.2884f, -390.59f, -162.5718f), 1, 0.01f, 0.5f);//climb up
                AfterDelay(1050, () =>
                {
                    jugg.ScriptModelPlayAnim(FallAnim);
                    jugg.MoveTo(new Vector3(484.8129f, -403.8325f, -394.4622f), 0.8f, 0.5f);//Falling off ledge
                    foreach (Entity players in Players)
                        JuggSpawnedHUD(players);
                    AfterDelay(850, () =>
                    {
                        jugg.ScriptModelPlayAnim(LandAnim);//We landed
                        AfterDelay(500, () =>
                        {
                            jugg.ScriptModelPlayAnim(IdleAnims[0]);//We are ready, use anim 0 as default
                            jugg.SetField("isActive", true);
                            SetJuggAI(jugg);//Startup the AI
                        });
                    });
                });
            }
            if (index == 11)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 11");
                jugg.ScriptModelPlayAnim(FallAnim);
                jugg.MoveGravity(new Vector3(200, 0, 200), 1);//fall off edge
                    foreach (Entity players in Players)
                        JuggSpawnedHUD(players);
                    AfterDelay(1050, () =>
                    {
                        jugg.ScriptModelPlayAnim(LandAnim);//We landed
                        AfterDelay(500, () =>
                        {
                            jugg.ScriptModelPlayAnim(IdleAnims[0]);//We are ready, use anim 0 as default
                            jugg.SetField("isActive", true);
                            SetJuggAI(jugg);//Startup the AI
                        });
                    });
            }
            if (index == 12)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 12");
                jugg.ScriptModelPlayAnim(WalkAnim);
                jugg.MoveTo(new Vector3(780.2791f, 2048.87f, -126.8646f), 0.3f);//Walking to edge
                AfterDelay(350, () =>
                {
                    jugg.ScriptModelPlayAnim(FallAnim);
                    jugg.MoveTo(new Vector3(653.3126f, 2051.202f, -251.3208f), 0.8f, 0.5f);//Falling off ledge
                    foreach (Entity players in Players)
                        JuggSpawnedHUD(players);
                    AfterDelay(850, () =>
                    {
                        jugg.ScriptModelPlayAnim(LandAnim);//We landed
                        AfterDelay(500, () =>
                        {
                            jugg.ScriptModelPlayAnim(IdleAnims[0]);//We are ready, use anim 0 as default
                            jugg.SetField("isActive", true);
                            SetJuggAI(jugg);//Startup the AI
                        });
                    });
                });
            }
            if (index == 13)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 13");
                jugg.ScriptModelPlayAnim(WalkAnim);
                jugg.MoveTo(new Vector3(390.8108f, 565.4866f, -173.8647f), 0.3f);//Walking to edge
                AfterDelay(350, () =>
                {
                    jugg.ScriptModelPlayAnim(FallAnim);
                    jugg.MoveTo(new Vector3(451.097f, 506.5069f, -292.8698f), 0.8f, 0.5f);//Falling off ledge
                    foreach (Entity players in Players)
                        JuggSpawnedHUD(players);
                    AfterDelay(850, () =>
                    {
                        jugg.ScriptModelPlayAnim(WalkAnim);//We landed
                        jugg.MoveTo(new Vector3(478.9842f, 490.7562f, -284.8656f), 0.4f);//Walking to edge
                        AfterDelay(450, () =>
                        {
                            jugg.ScriptModelPlayAnim(FallAnim);
                            jugg.MoveTo(new Vector3(581.2414f, 433.1337f, -387.1135f), 0.8f, 0.5f);//Falling off ledge
                            AfterDelay(850, () =>
                                {
                                    jugg.ScriptModelPlayAnim(LandAnim);
                                    AfterDelay(500, () =>
                                        {
                                            jugg.ScriptModelPlayAnim(IdleAnims[0]);//We are ready, use anim 0 as default
                                            jugg.SetField("isActive", true);
                                            SetJuggAI(jugg);//Startup the AI
                                        });
                                });
                        });
                    });
                });
            }
            if (index == 14)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 14");
                jugg.ScriptModelPlayAnim(WalkAnim);
                jugg.MoveTo(new Vector3(790.0021f, 891.3925f, -219.4595f), 0.5f);//Walking to edge
                AfterDelay(550, () =>
                {
                    jugg.ScriptModelPlayAnim(FallAnim);
                    jugg.MoveTo(new Vector3(803.046f, 970.3386f, -319.6552f), 0.8f, 0.5f);//Falling off ledge
                    foreach (Entity players in Players)
                        JuggSpawnedHUD(players);
                    AfterDelay(850, () =>
                    {
                        jugg.ScriptModelPlayAnim(LandAnim);//We landed
                        AfterDelay(500, () =>
                        {
                            jugg.ScriptModelPlayAnim(IdleAnims[0]);//We are ready, use anim 0 as default
                            jugg.SetField("isActive", true);
                            SetJuggAI(jugg);//Startup the AI
                        });
                    });
                });
            }
            if (index == 15)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 15");
                jugg.ScriptModelPlayAnim(WalkAnim);
                jugg.MoveTo(new Vector3(-842.8073f, 467.0091f, -279.8654f), 0.3f);//Walking to edge
                AfterDelay(350, () =>
                {
                    jugg.ScriptModelPlayAnim(FallAnim);
                    jugg.MoveTo(new Vector3(-779.4985f, 350.1683f, -411.875f), 0.8f, 0.5f);//Falling off ledge
                    foreach (Entity players in Players)
                        JuggSpawnedHUD(players);
                    AfterDelay(850, () =>
                    {
                        jugg.ScriptModelPlayAnim(LandAnim);//We landed
                        AfterDelay(500, () =>
                        {
                            jugg.ScriptModelPlayAnim(IdleAnims[0]);//We are ready, use anim 0 as default
                            jugg.SetField("isActive", true);
                            SetJuggAI(jugg);//Startup the AI
                        });
                    });
                });
            }
            if (index == 16)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 16");
                jugg.ScriptModelPlayAnim(WalkAnim);
                jugg.MoveTo(new Vector3(-385.1169f, -96.24047f, -174.3873f), 0.6f);//Walking to edge
                jugg.RotateTo(new Vector3(0, -65.96044f, 0), 0.6f, 0.1f, 0.5f);
                AfterDelay(650, () =>
                {
                    jugg.ScriptModelPlayAnim(LadderAnim);
                    jugg.MoveTo(new Vector3(-388.2324f, -93.75262f, -402.0828f), 3.5f);//climbing down
                    foreach (Entity players in Players)
                        JuggSpawnedHUD(players);
                    AfterDelay(3550, () =>
                    {
                        jugg.ScriptModelPlayAnim(IdleAnims[0]);//We landed
                        jugg.RotateTo(new Vector3(0, 111.6233f, 0), 0.6f, 0.1f, 0.5f);
                        AfterDelay(600, () =>
                        {
                            jugg.SetField("isActive", true);
                            SetJuggAI(jugg);//Startup the AI
                        });
                    });
                });
            }
            if (index == 17)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 17");
                jugg.ScriptModelPlayAnim(WalkAnim);
                jugg.MoveTo(new Vector3(471.3974f, 100.7467f, -205.5038f), 0.6f);//Walking to edge
                jugg.RotateTo(new Vector3(0, 115.3882f, 0), 0.6f, 0.1f, 0.5f);
                AfterDelay(650, () =>
                {
                    jugg.ScriptModelPlayAnim(LadderAnim);
                    jugg.MoveTo(new Vector3(470.8732f, 100.545f, -395.5162f), 3.5f);//climbing down
                    foreach (Entity players in Players)
                        JuggSpawnedHUD(players);
                    AfterDelay(3550, () =>
                    {
                        jugg.ScriptModelPlayAnim(IdleAnims[0]);//We landed
                        jugg.RotateTo(new Vector3(0, -62.81007f, 0), 0.6f, 0.1f, 0.5f);
                        AfterDelay(600, () =>
                        {
                            jugg.SetField("isActive", true);
                            SetJuggAI(jugg);//Startup the AI
                        });
                    });
                });
            }
            if (index == 18)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 18");
                jugg.ScriptModelPlayAnim(WalkAnim);
                jugg.MoveTo(new Vector3(-603.0576f, -195.9906f, -300.8649f), 0.3f);//Walking to edge
                AfterDelay(350, () =>
                {
                    jugg.ScriptModelPlayAnim(FallAnim);
                    jugg.MoveTo(new Vector3(-488.1904f, -80.90319f, -407.0691f), 0.8f, 0.5f);//Falling off ledge
                    jugg.RotateTo(new Vector3(0, 58.07361f, 0), 0.6f, 0.1f, 0.5f);
                    foreach (Entity players in Players)
                        JuggSpawnedHUD(players);
                    AfterDelay(850, () =>
                    {
                        jugg.ScriptModelPlayAnim(LandAnim);//We landed
                        AfterDelay(500, () =>
                        {
                            jugg.ScriptModelPlayAnim(IdleAnims[0]);//We are ready, use anim 0 as default
                            jugg.SetField("isActive", true);
                            SetJuggAI(jugg);//Startup the AI
                        });
                    });
                });
            }
            if (index == 19)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 19");
                jugg.ScriptModelPlayAnim(WalkAnim);
                jugg.MoveTo(new Vector3(-897.7486f, 1036.551f, -304.4703f), 0.8f, 0.3f);//Walking to edge
                AfterDelay(850, () =>
                {
                    jugg.ScriptModelPlayAnim(FallAnim);
                    jugg.MoveTo(new Vector3(-783.2646f, 1078.817f, -380.312f), 0.7f, 0.5f);//Falling off ledge
                    foreach (Entity players in Players)
                        JuggSpawnedHUD(players);
                    AfterDelay(750, () =>
                    {
                        jugg.ScriptModelPlayAnim(LandAnim);//We landed
                        AfterDelay(500, () =>
                        {
                            jugg.ScriptModelPlayAnim(IdleAnims[0]);//We are ready, use anim 0 as default
                            jugg.SetField("isActive", true);
                            SetJuggAI(jugg);//Startup the AI
                        });
                    });
                });
            }
            if (index == 20)
            {
                //Log.Write(LogLevel.All, "Starting init of jugg 20");
                jugg.ScriptModelPlayAnim(WalkAnim);
                jugg.MoveTo(new Vector3(1139.602f, -273.1647f, -165.5554f), 0.3f);//Walking to edge
                AfterDelay(350, () =>
                {
                    jugg.ScriptModelPlayAnim(FallAnim);
                    jugg.MoveTo(new Vector3(990.9218f, -191.4295f, -403.0862f), 0.8f, 0.5f);//Falling off ledge
                    foreach (Entity players in Players)
                        JuggSpawnedHUD(players);
                    AfterDelay(850, () =>
                    {
                        jugg.ScriptModelPlayAnim(LandAnim);//We landed
                        AfterDelay(500, () =>
                        {
                            jugg.ScriptModelPlayAnim(IdleAnims[0]);//We are ready, use anim 0 as default
                            jugg.SetField("isActive", true);
                            SetJuggAI(jugg);//Startup the AI
                        });
                    });
                });
            }
        }
        private void CheckStreak(Entity player)
        {
            int Killstreak = player.GetField<int>("killstreak");
            if (Killstreak == 5)
            {
                player.ShowHudSplash("airdrop_team_ammo", 0, 5);
                player.GiveWeapon("trophy_mp", 0, false);
                player.SetWeaponAmmoClip("trophy_mp", 1);
                player.GiveMaxAmmo("trophy_mp");
                player.SetActionSlot(5, "weapon", "trophy_mp");
                player.SetPlayerData("killstreaksState", "hasStreak", 1, true);
                player.SetPlayerData("killstreaksState", "countToNext", 8);
                player.SetPlayerData("killstreaksState", "nextIndex", 2);
                player.PlayLocalSound("mp_killstreak_carepackage");
            }
            else if (Killstreak == 8)
            {
                player.ShowHudSplash("deployable_vest", 0, 8);
                player.GiveWeapon("scrambler_mp", 0, false);
                player.SetWeaponAmmoClip("scrambler_mp", 1);
                player.GiveMaxAmmo("scrambler_mp");
                player.SetActionSlot(6, "weapon", "scrambler_mp");
                player.SetPlayerData("killstreaksState", "hasStreak", 2, true);
                player.PlayLocalSound("mp_killstreak_carepackage");
            }
        }
        public void CarePackText(Entity player, Entity crate)
        {
            /*
            HudElem message = HudElem.CreateFontString(player, HudElem.Fonts.Default, 1.6f);
            message.SetPoint("CENTER", "CENTER", 0, 110);
            //message.HideWhenInMenu = true;
            //message.Foreground = true;
            message.Alpha = .85f;
            message.Archived = true;
            message.Sort = 10;
            */
            OnInterval(100, () =>
            {
                if (crate == null || crateCount == 0)
                {
                    //message.Destroy();
                    player.ForceUseHintOff();
                    return false;
                }
                if (player.Origin.DistanceTo(crate.Origin) < 85 && !crate.HasField("isHealth"))
                {
                    player.ForceUseHintOn("Press ^3[{+activate}]^7 for Team Ammo Resupply");
                    waitForUseLeave(player, crate);
                    return false;
                }
                else if (player.Origin.DistanceTo(crate.Origin) < 85)
                {
                    player.ForceUseHintOn("Press ^3[{+activate}]^7 for Team Health Refill");
                    waitForUseLeave(player, crate);
                    return false;
                }
                else return true;
            });
        }

        public void waitForUseLeave(Entity player, Entity crate)
        {
            OnInterval(100, () =>
            {
                if (crate == null)
                {
                    player.ForceUseHintOff();
                    return false;
                }
                if (player.Origin.DistanceTo(crate.Origin) > 85)
                {
                    player.ForceUseHintOff();
                    CarePackText(player, crate);
                    return false;
                }
                return true;
            });
        }
        public void WatchCrate(Entity crate)
        {
            OnInterval(100, () =>
                {
                    //if (crate == null) return;
                    foreach (Entity p in Players)
                    {
                        if (!p.IsAlive) continue;
                    if (p.UseButtonPressed() && p.Origin.DistanceTo(crate.Origin) < 85 && crateCount != 0)
                        {
                            if (!crate.HasField("isHealth"))
                            {
                                crate.Delete();
                                crateCount--;
                                Objective_Delete(curObjID);
                                foreach (Entity players in Players)
                                {
                                    if (players.IsAlive)
                                    {
                                        players.GiveMaxAmmo("iw5_fad_mp_reflex_xmags_camo11");
                                        players.GiveMaxAmmo("at4_mp");
                                        players.GiveMaxAmmo("semtex_mp");
                                        players.GiveMaxAmmo("portable_radar_mp");
                                        //players.Call(33274, "secondaryoffhand", "death_nuke");
                                        Splash(p, players, "used_team_ammo_refill", true);
                                    }
                                }
                                crate = null;
                                return false;
                            }
                            else
                            {
                                crate.Delete();
                                crateCount--;
                                Objective_Delete(curObjID);
                                foreach (Entity players in Players)
                                {
                                    if (players.IsAlive)
                                    {
                                        players.Health = 999999;
                                        Splash(p, players, "Team Health Refill");
                                    }
                                    else
                                    {
                                        deadPlayers.Remove(players.Name);
                                        AfterDelay(500, () =>
                                            {
                                                players.Notify("menuresponse", "team_marinesopfor", "axis");
                                                AfterDelay(150, () => players.Notify("menuresponse", "changeclass", "axis_recipe1"));
                                                players.SetCardDisplaySlot(p, 5);
                                                players.ShowHudSplash("revived", 1);
                                            });
                                    }
                                }
                                crate = null;
                                return false;
                            }
                        }
                    }
                    return true;
                });
        }
        public void Splash(Entity caller, Entity players, string Splash, bool isLocalized = false)
        {
            if (!isLocalized)
            {
                HudElem splash = HudElem.CreateFontString(players, HudElem.Fonts.Objective, 1.5f);
                splash.SetPoint("TOPRIGHT", "TOPRIGHT", -5, 120);
                splash.GlowColor = new Vector3(0.5f, 0.5f, 0.5f);
                splash.GlowAlpha = 0.5f;
                splash.Color = new Vector3(0.2f, 0.7f, 0);
                HudElem splashname = HudElem.CreateFontString(players, HudElem.Fonts.HudBig, 0.8f);
                splashname.SetPoint("TOPRIGHT", "TOPRIGHT", -5, 100);
                splash.SetText(Splash);
                AfterDelay(3000, () => splash.Destroy());
                splashname.SetPlayerNameString(caller);
                AfterDelay(3000, () => splashname.Destroy());
                players.PlayLocalSound("mp_card_slide");
            }
            else
            {
                players.SetCardDisplaySlot(caller, 5);
                players.ShowHudSplash(Splash, 1);
            }
        }
    }
}
