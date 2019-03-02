using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSGSI;
using CSGSI.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Runtime.Serialization;

public class SteamAPIPlayer
{
    public string steamid { get; set; }
    public int communityvisibilitystate { get; set; }
    public int profilestate { get; set; }
    public string personaname { get; set; }
    public int lastlogoff { get; set; }
    public int commentpermission { get; set; }
    public string profileurl { get; set; }
    public string avatar { get; set; }
    public string avatarmedium { get; set; }
    public string avatarfull { get; set; }
    public int personastate { get; set; }
    public string primaryclanid { get; set; }
    public int timecreated { get; set; }
    public int personastateflags { get; set; }
}

public class SteamAPIResponse
{
    public List<SteamAPIPlayer> players { get; set; }
}

public class SteamAPIObject
{
    public SteamAPIResponse response { get; set; }
}


public class GSI_Receiver : MonoBehaviour {
    GameStateListener gsl;

    dynamic PlayerState;
    //public string[] PlayerState1;
    //public Dictionary<string, object> PlayerState2;
    //public Dictionary<string, object> PlayerState3;

    public string Steam_API_Key;
    public string GSI_Auth;

    // general informations,such as rounds,phases,maps,etc
    public string gsi_phase;
    public string gsi_map;
    // public string gsi_team1;
    // public string gsi_team2;
    public int gsi_team1_score;
    public int gsi_team2_score;

    // all players information
    public object gsi_all_players;
    public object[] gsi_all_players_array;
    public string gsi_all_players_string;
    public string gsi_auth;

    // main informations who you observing at
    public int spec_hp;
    public int spec_ap;
    public bool spec_has_helmet;
    public int spec_ammo_loaded;
    public int spec_ammo_max;
    public int spec_ammo_reserve;
    public int spec_stats_kills;
    public int spec_stats_deaths;
    public int spec_stats_assists;
    public string spec_name;
    public string spec_team;
    public string spec_weapon_active;
    public string[] spec_greandes;
    public string spec_steamid;
    private string spec_previous_steamid;
    private bool refresh_avatar;

    // to link other Unity components
    public Text comp_spec_name;
    public Text comp_spec_hp;
    public Text comp_spec_ap;
    public RawImage comp_spec_avatar;
    public RawImage comp_spec_helmet;
    public Text comp_spec_ammo_loaded;
    public Text comp_spec_ammo_max;
    public Text comp_spec_ammo_reserve;
    public Text comp_spec_stats_kills;
    public Text comp_spec_stats_assists;
    public Text comp_spec_stats_deaths;

    public Text comp_score_team1;
    public Text comp_score_team2;
    //public Text[] comp_players_name = new Text[10];
    //public Dictionary<string, Text> comp_players_name;
    public Text[] comp_players_name_array = new Text[10];
    private Dictionary<string, object> gsi_all_players_dict;

    /*
    public Text comp_; // etc
    */

    IEnumerator GetAvatar()
    {
        Debug.Log("GET AVATAR");
        refresh_avatar = false;
        var req_url = "http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=" + Steam_API_Key + "&steamids=" + spec_steamid;
        UnityWebRequest SteamProfileData = UnityWebRequest.Get(req_url);
        Debug.Log(req_url);
        // http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=XXXXXXXXXXXXXXXXXXXXXXX&steamids=76561197960435530
        yield return SteamProfileData.SendWebRequest();
        if (SteamProfileData.isHttpError || SteamProfileData.isNetworkError)
        {
            //4.エラー確認
            Debug.Log("Request profileimg error : " + SteamProfileData.error);
        }
        else
        {
            var SteamProfileData_res = SteamProfileData.downloadHandler.text;
            // RawImageに取得したテクスチャを代入

            var result = JsonConvert.DeserializeObject<SteamAPIObject>(SteamProfileData_res);

            //var SteamProfileData_dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(SteamProfileData_res);
            //var SteamProfileData_url = JsonConvert.DeserializeObject<Dictionary<string, object>>(SteamProfileData_dict["renponse"].ToString());
            //Debug.Log("RES : " + SteamProfileData_dict["renponse"].ToString());
            //result.response.players[0].avatarfull;
            Debug.Log("successfully get url,URL : " + result.response.players[0].avatarfull);

            UnityWebRequest profileimg = UnityWebRequestTexture.GetTexture(result.response.players[0].avatarfull);
            yield return profileimg.SendWebRequest();
            if (profileimg.isHttpError || profileimg.isNetworkError)
            {
                //4.エラー確認
                Debug.Log("Request profileimg error : " + profileimg.error);
            }
            else
            {
                comp_spec_avatar.texture = ((DownloadHandlerTexture)profileimg.downloadHandler).texture; // RawImageに取得したテクスチャを代入
            }
        }
    }

    void Start () {
        gsl = new GameStateListener("http://127.0.0.1:3001");
        gsl.NewGameState += new NewGameStateHandler(OnNewGameState);
        if (!gsl.Start())
        {
            Debug.Log("GSI Server failed");
        }
        Debug.Log("GSI Server launched!");
    }
	
	// Update is called once per frame
	void Update () {
        if (gsl.Running)
        {

            comp_spec_name.text = spec_name.ToString();
            comp_spec_hp.text = spec_hp.ToString();
            comp_spec_ap.text = spec_ap.ToString();

            comp_score_team1.text = Convert.ToString(gsi_team1_score);
            comp_score_team2.text = Convert.ToString(gsi_team2_score);


            comp_spec_ammo_loaded.text = spec_ammo_loaded.ToString();
            comp_spec_ammo_reserve.text = spec_ammo_reserve.ToString();
            comp_spec_ammo_max.text = spec_ammo_max.ToString();
            comp_spec_stats_kills.text = Convert.ToString(spec_stats_kills);
            comp_spec_stats_deaths.text = Convert.ToString(spec_stats_deaths);
            comp_spec_stats_assists.text = Convert.ToString(spec_stats_assists);

            if (refresh_avatar)
            {
                StartCoroutine("GetAvatar");
            }
            if (gsi_all_players_dict != null)
            {
                var gsi_all_players_upd = gsi_all_players_dict;
                foreach (var key in gsi_all_players_upd.Keys)
                {
                    //switch doesnt suupport dynamic types :(
                    /*
                    switch(PlayerState[key].observerslot)
                    {

                    }
                    */
                    Debug.Log("Observer slot : " + PlayerState[key].observer_slot);
                    Debug.Log("Name : " + PlayerState[key].name);

                    if (PlayerState[key].observer_slot == "0")
                    {
                        comp_players_name_array[0].text = PlayerState[key].name; // これが正解
                    }
                    else if (PlayerState[key].observer_slot == "1")
                    {
                        comp_players_name_array[1].text = PlayerState[key].name; // これが正解
                    }
                    else if (PlayerState[key].observer_slot == "2")
                    {
                        comp_players_name_array[2].text = PlayerState[key].name; // これが正解
                    }
                    else if (PlayerState[key].observer_slot == "3")
                    {
                        comp_players_name_array[3].text = PlayerState[key].name; // これが正解
                    }
                    else if (PlayerState[key].observer_slot == "4")
                    {
                        comp_players_name_array[4].text = PlayerState[key].name; // これが正解
                    }
                    else if (PlayerState[key].observer_slot == "5")
                    {
                        comp_players_name_array[5].text = PlayerState[key].name; // これが正解
                    }
                    else if (PlayerState[key].observer_slot == "6")
                    {
                        comp_players_name_array[6].text = PlayerState[key].name; // これが正解
                    }
                    else if (PlayerState[key].observer_slot == "7")
                    {
                        comp_players_name_array[7].text = PlayerState[key].name; // これが正解
                    }
                    else if (PlayerState[key].observer_slot == "8")
                    {
                        comp_players_name_array[8].text = PlayerState[key].name; // これが正解
                    }
                    else if (PlayerState[key].observer_slot == "9")
                    {
                        comp_players_name_array[9].text = PlayerState[key].name; // これが正解
                    }


                    Debug.Log("KEY : " + key + ",State : " + PlayerState[key]);
                    //Debug.Log("KEY : " + key + ",Name : " + PlayerState[key].name);
                    //comp_players_name[key].text = PlayerState[key].name;
                    //comp_players_name[1].text = PlayerState[1].name;
                    //comp_players_name[2].text = PlayerState[2].name;
                    //comp_players_name[3].text = PlayerState[3].name;
                    //comp_players_name[4].text = PlayerState[4].name;
                    //comp_players_name[5].text = PlayerState[5].name;

                    /*
                    Debug.Log("SteamID : " + key);
                    Debug.Log("dic : " + gsi_all_players[key]);
                    //var gsi_all_players_state = JsonConvert.DeserializeObject(gsi_all_players[key]);
                    PlayerState[key] = JsonConvert.DeserializeObject(gsi_all_players[key].ToString());
                    Debug.Log("KEY[" + key + "]'s name is : " + PlayerState[key].name);
                    */
                }
            }
        }
    }

    void OnNewGameState(GameState gs)
    {
        /*
        if(gs.Auth.ToString() != gsi_auth)
        {
            Debug.LogError("GSI Auth token error...");
        }
        */
        if (gs.Player.SteamID != spec_steamid)
        {
            spec_steamid = gs.Player.SteamID;
            refresh_avatar = true;
            //StartCoroutine("GetAvatar");
            Debug.Log("Requesting new avatar image");
            Debug.Log("Previous : " + spec_steamid);
            Debug.Log("Now : " + gs.Player.SteamID);
        }
        else
        {
            refresh_avatar = false;
        }

        gsi_phase = gs.Round.Phase.ToString();
        gsi_map = gs.Map.Name;

        // gsi_team1 = gs.Map.TeamCT.JSON; // チーム名
        gsi_team1_score = gs.Map.TeamCT.Score;
        // gsi_team2 = gs.Map.TeamT.JSON; // チーム名
        gsi_team2_score = gs.Map.TeamT.Score;

        //gsi_all_players = JsonConvert.DeserializeObject(gs.AllPlayers.JSON);
        gsi_all_players_string = gs.AllPlayers.JSON;
        
        var gsi_all_players = JsonConvert.DeserializeObject<Dictionary<string, object>>(gsi_all_players_string);
        gsi_all_players_dict = gsi_all_players;
        PlayerState = JsonConvert.DeserializeObject(gsi_all_players_string);
        //var gsi_all_players_state = JsonConvert.DeserializeObject<Dictionary<string, string>>(gsi_all_players_string + "[state]");
        //var gsi_all_players_match_stats = JsonConvert.DeserializeObject<Dictionary<string, string>>(gsi_all_players_string + "match_stats");


        gsi_auth = gs.Auth.Token;

        spec_hp = gs.Player.State.Health;
        spec_ap = gs.Player.State.Armor;
        spec_has_helmet = gs.Player.State.Helmet;
        spec_ammo_loaded = gs.Player.Weapons.ActiveWeapon.AmmoClip;
        spec_ammo_max = gs.Player.Weapons.ActiveWeapon.AmmoClipMax;
        //spec_ammo_reserve = gs.Player.Weapons.ActiveWeapon.AmmoReserve;
        spec_stats_kills = gs.Player.MatchStats.Kills;
        spec_stats_deaths = gs.Player.MatchStats.Deaths;
        spec_stats_assists = gs.Player.MatchStats.Assists;

        spec_name = gs.Player.Name;
        spec_weapon_active = gs.Player.Weapons.ActiveWeapon.Name;
        spec_team = gs.Player.Team;
        spec_steamid = gs.Player.SteamID;
        spec_previous_steamid = gs.Previously.Player.SteamID;

        bool winner_scene_loaded = false;
        if (gs.Round.Phase.ToString() == "Over")
        {
            //Debug.Log(gs.Round.WinTeam.ToString());
            if (!winner_scene_loaded)
            {
                switch (gs.Round.WinTeam.ToString())
                {
                    case "CT":
                        Debug.Log("CT wins");
                        SceneManager.LoadScene("CTWin", LoadSceneMode.Additive);
                        winner_scene_loaded = true;
                        break;
                    case "T":
                        Debug.Log("T wins");
                        SceneManager.LoadScene("TWin", LoadSceneMode.Additive);
                        winner_scene_loaded = true;
                        break;
                    case "default":
                        Debug.Log("Unknown");
                        winner_scene_loaded = false;
                        break;
                }
            }
        }
        else if (gs.Round.Phase.ToString() == "FreezeTime")
        {
            Debug.Log("Freeze time now");
            winner_scene_loaded = false;
        }
    }
}
