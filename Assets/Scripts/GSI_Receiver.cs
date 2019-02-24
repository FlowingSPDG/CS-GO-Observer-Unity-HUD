using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSGSI;
using CSGSI.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class GSI_Receiver : MonoBehaviour {
    GameStateListener gsl;

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
    public string spec_name;
    public string spec_team;
    public string spec_weapon_active;
    public string[] spec_greandes;
    public string spec_steamid;


    // to link other Unity components
    public Text comp_spec_name;
    public Text comp_spec_hp;
    public Text comp_spec_ap;
    /*
    public Text comp_; // etc
    */

    void Start () {
        gsl = new GameStateListener("http://192.168.1.14:3001");
        gsl.NewGameState += new NewGameStateHandler(OnNewGameState);
        if (!gsl.Start())
        {
            Debug.Log("GSI Server failed");
        }
        Debug.Log("GSI Server launched!");
    }
	
	// Update is called once per frame
	void Update () {
        comp_spec_name.text = spec_name.ToString();
        comp_spec_hp.text = spec_hp.ToString();
        comp_spec_ap.text = spec_ap.ToString();
    }

    void OnNewGameState(GameState gs)
    {
        gsi_phase = gs.Round.Phase.ToString();
        gsi_map = gs.Map.Name;

        // gsi_team1 = gs.Map.TeamCT.JSON; // チーム名
        gsi_team1_score = gs.Map.TeamCT.Score;
        // gsi_team2 = gs.Map.TeamT.JSON; // チーム名
        gsi_team2_score = gs.Map.TeamT.Score;

        //gsi_all_players = JsonConvert.DeserializeObject(gs.AllPlayers.JSON);
        gsi_all_players_string = gs.AllPlayers.JSON;
        
        var gsi_all_players = JsonConvert.DeserializeObject<Dictionary<string, object>>(gsi_all_players_string);
        //var gsi_all_players_state = JsonConvert.DeserializeObject<Dictionary<string, string>>(gsi_all_players_string + "[state]");
        //var gsi_all_players_match_stats = JsonConvert.DeserializeObject<Dictionary<string, string>>(gsi_all_players_string + "match_stats");

        foreach (var key in gsi_all_players.Keys)
        {
            Debug.Log("SteamID : " + key);
            Debug.Log("dic : " + gsi_all_players[key]);
            //var gsi_all_players_state = JsonConvert.DeserializeObject(gsi_all_players[key]);
        }


        gsi_auth = gs.Auth.Token;

        spec_hp = gs.Player.State.Health;
        spec_ap = gs.Player.State.Armor;
        spec_name = gs.Player.Name;
        spec_weapon_active = gs.Player.Weapons.ActiveWeapon.Name;
        spec_team = gs.Player.Team;
        spec_steamid = gs.Player.SteamID;
    }
}
