using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{
    [SerializeField] AudioSource cash_sound;
    [SerializeField] AudioSource error;

    // Elementy interfejsu
    // Informacja o złocie, turach, oraz nazwa aktualnego gracza 
    // i jego punkty zadań
    public Text QuestPoints;
    public Text WinPoints;
    public Text Gold;
    public Text turns;
    public Text maxTurns;
    public Text PlayerName;
    // dziennik logów
    public GameObject MessageBox;
    private string logText;
    // licznik tur
    public float turnsCounter = 1;
    // Panele planet
    [SerializeField] private GameObject CityPanel;
    [SerializeField] private GameObject MinePanel;
    [SerializeField] private GameObject ExplorePlanetPanel;
    [SerializeField] private GameObject TakeOverPlanetPanel;
    [SerializeField] private GameObject StandardInterface;

    [SerializeField] private GameObject EndGamePanel;
    [SerializeField] private GameObject EndPlayerPanel;
    // Obecny gracz,Obecnie zaznaczony obiekt
    public Player currentPlayer;
    public GameObject SelectedObject;
    // liczba graczy, tabela indeksów planet startowych
    private int PlayersCount;
    int[] tab;
    public bool Pause { get; set; }
    // wskaźniki do obiektów typu:kamera, 
    [SerializeField] private GameObject Cam;
    //Pojemnik na graf (mapę) -- klasa
    [SerializeField] private GraphContainer graph;
    // wyzwalacz walki -- klasa
    [SerializeField] private BattleHandler battle;
    // Pojemnik na graczy oraz ich nazwy -- klasa
    [SerializeField] public NamesHolder namesHolder;

    [SerializeField]  ClampPanel PanelLogic;
    [SerializeField] GameObject PlusIcon;

    public GameObject kurtyna;

    public static GameLogic Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        PlayersCount = namesHolder.PlayerNumber();
        Debug.LogError("liczba graczy " + PlayersCount);
        if (PlayersCount <= 0)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        namesHolder.InitPlayers();
        graph.Initialize(PlayersCount);
        namesHolder.SetStartPositions(graph);

        currentPlayer = namesHolder.ReturnNextPlayer().GetComponent<Player>();
        currentPlayer.GetComponent<Player>().IsActive = true;
        ResetPlanetAvailability();
        Cam.GetComponent<CameraMovement>().ChangePosition(currentPlayer.GetComponent<Player>().ReturnPawn().transform);
        UpdateLogs();
        string str = graph.db.Query("match (n:SpaceSystem) return min(n.x),max(n.x),min(n.y),max(n.y)");
        Debug.Log(str); //string str = db.Query("match (n:SpaceSystem) return min(n.x),max(n.x),min(n.y),max(n.y)");
        maxTurns.text = "/"+ (namesHolder.GetTurnLimit());

        foreach (var item in namesHolder.PlayerList)
        {
            Debug.LogWarning(""+ item.GetComponent<Player>().Tactics);
        }


        setKurtyna();
    }

    bool Ended = false;

    void Update()
    {
        if (EndTurnPause || Ended)
        {
            return;
        }

        if (!currentPlayer.Bot)
        {
            if (!Pause)
            {
                HandleInputs();
            }
        }
        else
        {
            Botcontrol();
        }
        //PanelLogic.LastSelectedObject = SelectedObject;
    }

    void HandleInputs()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentPlayer.goldAmount = 500;
            UpdateLogs();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null)
            {
                if (hit.collider.tag == "Planet")
                {
                    //path
                    FindTheWay(currentPlayer.ReturnPawn().GetComponent<Pawn>().ReturnCurrentPlanet().GetComponent<Planet>(),hit.collider.gameObject.GetComponent<Planet>());
                }
            }
        }


            //----------------------------------

            // Odznacz aktualnie wybrany obiekt
            if (Input.GetMouseButtonDown(1) && SelectedObject != null)
        {
            //currentPlayer.GetComponent<Player>().goldAmount = 1000;
            Debug.LogError(currentPlayer.GetComponent<Player>().MotherPlanetList.Count);
            UpdateLogs();
            UnselectSelectedObject();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            // najpierw sprawdz czy klikam jakis obiekt
            if (hit.collider != null)
            {
                //-------------------------------------------------------
                // sprawdz czy wybrany (kliknięty) obiekt to pionek
                //-------------------------------------------------------

                if (hit.collider.gameObject.tag == "Pawn")
                {
                    if (hit.collider.gameObject.GetComponent<Pawn>().Player.GetComponent<Player>().PlayerNr != currentPlayer.GetComponent<Player>().PlayerNr)
                    {
                    }
                    else if (SelectedObject == null)
                    {
                        SelectedObject = hit.collider.gameObject;
                        SelectedObject.GetComponent<Pawn>().TurnLightOn();
                    }
                    else if (hit.collider.gameObject == SelectedObject)
                    {
                        SelectedObject.GetComponent<Pawn>().TurnLightOff();
                        SelectedObject = null;
                    }
                    else if (hit.collider.gameObject != SelectedObject)
                    {
                        if (SelectedObject.tag == "Planet")
                        {
                            SelectedObject.GetComponent<Planet>().TurnLightOff();
                            SelectedObject = hit.collider.gameObject;
                            SelectedObject.GetComponent<Pawn>().TurnLightOn();
                        }
                        else if (SelectedObject.tag == "Pawn")
                        {
                            SelectedObject.GetComponent<Pawn>().TurnLightOff();
                            SelectedObject = hit.collider.gameObject;
                            SelectedObject.GetComponent<Pawn>().TurnLightOn();
                            //--------------------------------
                            // walka też powinna być tu
                            // -------------------------------
                        }
                    }

                }
                // ------------------- KLIKNIĘCIE NA PLANETĘ ---------------------
                else if (hit.collider.gameObject.tag == "Planet")
                {
                    // klikam na planetę i .......
                    if (SelectedObject == null) // do tej pory nie miałem nic zaznaczone 
                    {
                        SelectedObject = hit.collider.gameObject;
                        SelectedObject.GetComponent<Planet>().TurnLightOn();
                    }
                    else if (hit.collider.gameObject == SelectedObject) // mma już zaznaczoną tę planetę
                    {
                        SelectedObject.GetComponent<Planet>().TurnLightOff();
                        SelectedObject = null;
                    }
                    else if (hit.collider.gameObject != SelectedObject) // Mam już zaznaczony inny obiekt
                    {
                        // inny zaznaczony obiekt jest planetą
                        if (SelectedObject.tag == "Planet")
                        {
                            SelectedObject.GetComponent<Planet>().TurnLightOff();
                            SelectedObject = hit.collider.gameObject;
                            SelectedObject.GetComponent<Planet>().TurnLightOn();
                        }
                        // inny zaznaczony obiekt jest pionkiem (mam zaznaczony pionek i klikam na planetę)
                        else if (SelectedObject.tag == "Pawn" /* && pawn jest aktualnego gracza */)
                        {
                            PawnMove(SelectedObject, hit.collider.gameObject);

                           // ExplorePlanet(hit.collider.gameObject);
                        }
                    }

                    //------------------------------------
                    // tutaj kod obsługi wybranej planety
                    //------------------------------------

                    // jeśli zaznaczony obiekt nie jest NULL
                    if (SelectedObject != null && SelectedObject.tag == "Planet")
                    {
                        // Jeśli nie znam planety
                        if (!currentPlayer.GetComponent<Player>().PlanetList.Contains(SelectedObject))
                        {
                            // jeśli mam na planecie pionek
                            if (SelectedObject.GetComponent<Planet>().GetPawn() != null)
                            {
                                if (currentPlayer.GetComponent<Player>().ReturnPawn() == SelectedObject.GetComponent<Planet>().GetPawn().gameObject)
                                {
                                    // wyświetl panel ODKRYCIA PLANETY
                                    PanelLogic.LastSelectedObject = SelectedObject;
                                    gameObject.GetComponent<ButtonAction>().TooglePanel(ExplorePlanetPanel);
                                }
                            }
                        }
                        else// znam tę planetę
                        {
                            // jeśli obiekt należy do mnie
                            if (currentPlayer.GetComponent<Player>().PlayerNr == SelectedObject.GetComponent<Planet>().OwnerNumber)
                            {
                                // otwórz panel miasta
                                if (SelectedObject.GetComponent<Planet>().Type == "City")
                                {
                                    // ustaw odpowiednio tekst na panelu
                                    RefreshCityPanel();
                                    //Text[] txt = CityPanel.GetComponentsInChildren<Text>();
                                    //txt[2].text = "Civilization Level: " + SelectedObject.GetComponent<MotherPlanet>().civilizationLevel;
                                    //txt[3].text =""+ SelectedObject.GetComponent<MotherPlanet>().ReturnCivilizationCost();  

                                    // otwórz panel
                                    gameObject.GetComponent<ButtonAction>().TooglePanel(CityPanel);
                                }
                                else if (SelectedObject.GetComponent<Planet>().Type == "Mine")
                                {
                                    RefreshMinePanel();
                                    gameObject.GetComponent<ButtonAction>().TooglePanel(MinePanel);
                                }
                            }// jeśli nie należy do mnie, ale za to jest na nim mój pionek
                            else if (currentPlayer.GetComponent<Player>().ReturnPawn() == SelectedObject.GetComponent<Planet>()?.GetPawn()?.gameObject)
                            {
                                // wyświetl panel ODKRYCIA PLANETY
                                gameObject.GetComponent<ButtonAction>().TooglePanel(TakeOverPlanetPanel);
                            }

                        }

                    }
                }
                else if (hit.collider.gameObject.tag == "Location")
                {
                    //if()
                    if (SelectedObject?.tag == "Pawn" /* && pawn jest aktualnego gracza */)
                    {
                        if(SelectedObject.GetComponent<Pawn>().Player.GetComponent<Player>().PlayerNr == currentPlayer.GetComponent<Player>().PlayerNr)
                        PawnMove(SelectedObject, hit.collider.gameObject);
                    }
                }
            }
        }
    }

    public void ButtonTest()
    {
        graph.MapGeneration2(50);
    }

    public void UnselectSelectedObject()
    {
        if (SelectedObject != null)
        {
            if (SelectedObject.tag == "Pawn")
            {
                SelectedObject.GetComponent<Pawn>().TurnLightOff();
                SelectedObject = null;
            }
            else if (SelectedObject.tag == "Planet")
            {
                SelectedObject.GetComponent<Planet>().TurnLightOff();
                SelectedObject = null;
            }
            else if (SelectedObject.tag == "Location")
            {
                SelectedObject = null;
            }
            SelectedObject = null;
        }
    }

    public void TakeOverPlanet()
    {
        if (currentPlayer.GetComponent<Player>().QuestPoints > 0
            && currentPlayer.GetComponent<Player>().ReturnPawn() == SelectedObject.GetComponent<Planet>().GetPawn().gameObject)
        {
            // SelectedObject.GetComponent<Location>().ShowPlanet();
            //if (planet.GetComponent<MotherPlanet>())
            //{
            //    currentPlayer.GetComponent<Player>().MotherPlanetList.Add(planet.GetComponent<MotherPlanet>());
            //}

            SelectedObject.GetComponent<Planet>().ChangeOwner(currentPlayer.GetComponent<Player>().PlayerNr);
                currentPlayer.GetComponent<Player>().QuestPoints--;
                UpdateLogs();         
        }
    }

    public void ExplorePlanet(GameObject planet)
    {
        if (!currentPlayer.GetComponent<Player>().PlanetList.Contains(planet) /*&& planet.GetComponent<Location>().GetPawn().GetComponent<Pawn>().GetActionPoints() > 0*/)
        {
            currentPlayer.GetComponent<Player>().PlanetList.Add(planet);

            if(planet.GetComponent<Planet>())
            planet.GetComponent<Planet>().ShowPlanet();

            UpdateLogs();
        }
    }

    public void RefreshMinePanel()
    {
        Text[] txt = MinePanel.GetComponentsInChildren<Text>();
        txt[2].text = "Mine Level: " + SelectedObject.GetComponent<MinePlanet>().MineLevel;
        txt[3].text = "" + SelectedObject.GetComponent<MinePlanet>().ReturnUpgradeCost();
    }// odświeżanie panelu kopalni

    public void RefreshCityPanel()
    {
        Text[] txt = CityPanel.GetComponentsInChildren<Text>();
        //txt[0].text; - buildings

        txt[1].text = "Civilization Level: " + SelectedObject.GetComponent<MotherPlanet>().civilizationLevel;
        txt[2].text = "" + SelectedObject.GetComponent<MotherPlanet>().ReturnCivilizationCost();

        //txt[3].text = ""; - upgrade button
        //txt[4].text = ""; - actions

        // cost of ship upgrade
        //Debug.LogError("refresh");
        if (SelectedObject.GetComponent<MotherPlanet>().IsPawnHere())
        {
            if (SelectedObject.GetComponent<MotherPlanet>().GetPawn().Player == currentPlayer.gameObject)
            {
                if (SelectedObject.GetComponent<MotherPlanet>().GetPawn().power < 5)
                {
                    txt[8].text = "" + SelectedObject.GetComponent<MotherPlanet>().GetPawn().ReturnUpgradeCost();
                }
                else
                {
                    txt[8].text = "Max";
                }
            }
            else
            {
                txt[8].text = "---";
            }
        }
        else
        {
            txt[8].text = "---";
        }

        //txt[7].text =
    }// odświeżanie panelu miasta

    public void CivilUpgrade()
    {

        if (currentPlayer.GetComponent<Player>().goldAmount >= SelectedObject.GetComponent<MotherPlanet>().ReturnCivilizationCost()
            && !SelectedObject.GetComponent<MotherPlanet>().maxCivilLevel()
            && currentPlayer.GetComponent<Player>().QuestPoints > 0)
        {
            // odbierz graczowi pieniądze
            currentPlayer.GetComponent<Player>().goldAmount = -SelectedObject.GetComponent<MotherPlanet>().ReturnCivilizationCost();
            // odbierz punkt zadań
            currentPlayer.GetComponent<Player>().QuestPoints--;
            // ulepsz POZIOM CYWILIZACJI
            SelectedObject.GetComponent<MotherPlanet>().UpgradeCivilization();
            cash_sound.Play();
            // odśwież informacje na INTERFEJSIE
            RefreshCityPanel();
            UpdateLogs();

            if(!currentPlayer.Bot)
            ShowMessage(2f,"City level upgraded!");
            
        }
        else
        {
            if (!currentPlayer.Bot)
            { 
                ShowMessage(3f, "Not enough gold or city level is max!");
                error.Play();
            }
            //Debug.Log("Za mało złota lub poziom miasta jest maksymalny!");
        }
    } // funkcja przypisana guzikowi ULEPSZ CIWILIZACJE miasta w panelu CITY PANEL [BUTTON]

    public void MineUpgrade()
    {
        if (currentPlayer.GetComponent<Player>().goldAmount >= SelectedObject.GetComponent<MinePlanet>().ReturnUpgradeCost()
            && !SelectedObject.GetComponent<MinePlanet>().maxMineLevel()
            && currentPlayer.GetComponent<Player>().QuestPoints > 0)
        {
            // odbierz graczowi pieniądze
            currentPlayer.GetComponent<Player>().goldAmount = -SelectedObject.GetComponent<MinePlanet>().ReturnUpgradeCost();
            // odbierz punkt zadań
            currentPlayer.GetComponent<Player>().QuestPoints--;
            // ulepsz POZIOM CYWILIZACJI
            SelectedObject.GetComponent<MinePlanet>().UpgradeMine();
            cash_sound.Play();
            // odśwież informacje na INTERFEJSIE
            RefreshMinePanel();
            UpdateLogs();
            //Debug.Log("Ulepszono kopalnię !");
            if(!currentPlayer.Bot)
            ShowMessage(2f, "Mine level upgraded!");
        }
        else
        {
            if (!currentPlayer.Bot)
            { 
                ShowMessage(3f,"Not enough gold or mine level is max!");
            error.Play();
            }
        }
    }  // analogicznie dla MINE PANEL [BUTTON]

    public void UpgradeSpaceShipBot()
    {
        if (currentPlayer.GetComponent<Player>().ReturnPawn().GetComponent<Pawn>().power < 5)
        {
            if (currentPlayer.GetComponent<Player>().goldAmount >= currentPlayer.ReturnPawn().GetComponent<Pawn>().ReturnUpgradeCost()
                && currentPlayer.GetComponent<Player>().QuestPoints >= 1)
            {
                currentPlayer.GetComponent<Player>().goldAmount = -currentPlayer.GetComponent<Player>().ReturnPawn().GetComponent<Pawn>().ReturnUpgradeCost();
                currentPlayer.GetComponent<Player>().QuestPoints--;
                currentPlayer.ReturnPawn().GetComponent<Pawn>().power++;

                //cash_sound.Play();
                //RefreshCityPanel();
                UpdateLogs();
            }
            else
            {

            }
        }
    }

    public void UpgradeSpaceShip()
    {
        if (SelectedObject.GetComponent<MotherPlanet>().IsPawnHere())
        {
            if (SelectedObject.GetComponent<MotherPlanet>().GetPawn().Player == currentPlayer.gameObject)
            {
                if (SelectedObject.GetComponent<MotherPlanet>().GetPawn().power < 5)
                {
                    if (currentPlayer.GetComponent<Player>().goldAmount >= SelectedObject.GetComponent<MotherPlanet>().GetPawn().ReturnUpgradeCost()
                        && currentPlayer.GetComponent<Player>().QuestPoints >= 1)
                    {
                        currentPlayer.GetComponent<Player>().goldAmount = -SelectedObject.GetComponent<MotherPlanet>().GetPawn().ReturnUpgradeCost();
                        currentPlayer.GetComponent<Player>().QuestPoints--;
                        SelectedObject.GetComponent<MotherPlanet>().GetPawn().power++;
                        cash_sound.Play();
                        ShowMessage(3f,"Ship upgraded, new power level is "+ SelectedObject.GetComponent<MotherPlanet>().GetPawn().power);
                        RefreshCityPanel();
                        UpdateLogs();
                    }
                    else
                    {
                        ShowMessage(2f, "Not enough money or action points!");
                        error.Play();
                    }
                }
                else
                {
                    ShowMessage(2f,"max power reached");
                    error.Play();
                }
            }
        }
        else
        {
            ShowMessage(3f, "Dock the ship you want to upgrade.");
            error.Play();
        }
    }

    public void Research() 
    {// badanie kosmosu, czyli dobudowanie nowego systemu
        if (NamesHolder.mapMode == MapMode.separate)
        {
            //================
            if (SelectedObject.GetComponent<MotherPlanet>().civilizationLevel < 2)
            { return; }
            if (currentPlayer.GetComponent<Player>().QuestPoints > 0 && currentPlayer.GetComponent<Player>().goldAmount >= 500)
            {
                if (graph.AddNewSectorSeparationMod(currentPlayer))
                {
                    currentPlayer.GetComponent<Player>().QuestPoints--;
                    currentPlayer.GetComponent<Player>().goldAmount = -500;
                    //cash_sound.Play();
                    ResetPlanetAvailability();
                    UpdateLogs();
                }
                else
                {
                    if (!currentPlayer.Bot)
                    {
                        error.Play();
                        ShowMessage(3f, "You already discovered all map.");
                    }
                }
            }



        }
        else if(NamesHolder.mapMode == MapMode.normal || NamesHolder.mapMode == MapMode.shared )
        {
            if (SelectedObject.GetComponent<MotherPlanet>().civilizationLevel < 2)
            {
                if (!currentPlayer.Bot)
                {
                    error.Play();
                    ShowMessage(3f, "You need to have civilisation level equal or higher than 2.");
                }
                return;
            }

            if (currentPlayer.GetComponent<Player>().QuestPoints > 0 && currentPlayer.GetComponent<Player>().goldAmount >= 500)
            {
                while (true)
                {
                    if (graph.AddNewSector(currentPlayer))
                    {
                        currentPlayer.GetComponent<Player>().QuestPoints--;
                        currentPlayer.GetComponent<Player>().goldAmount = -500;
                        cash_sound.Play();
                        ResetPlanetAvailability();
                        UpdateLogs();

                        if (!currentPlayer.Bot)
                        { ShowMessage(2f, "New space system founded."); }

                        break;
                    }
                }
                Debug.LogError("found new system");
            }
            else
            {
                Debug.LogError("bot cannot research, gold: " + currentPlayer.goldAmount + "quest points: " + currentPlayer.QuestPoints);
                if (!currentPlayer.Bot)
                {
                    ShowMessage(2f, "You don't have any action points or enough amount of gold!");
                    error.Play();
                }
            }
        }
    }

    public bool AddTurns = false;
    public Dropdown NewPlayerOption;

    public void AddNewPlayer(GameObject item)
    {
        bool bot = false;
            string str = item.GetComponentInChildren<InputField>().text;
            if (str != "")
            namesHolder.Add(str, bot,Strategy.casual);
            else
                namesHolder.Add("Unknown", bot, Strategy.casual);

        namesHolder.AddPlayer(PlayersCount, bot, Strategy.casual);
        PlayersCount = namesHolder.PlayerNumber();

        if (NewPlayerOption.value == 0)
        {
            GameObject tmp = graph.CreateSpaceForNewPlayer();
            namesHolder.SetNewPlayerPosition(tmp);

            int val = MeanPlayersWinPoints(namesHolder.ActivePlayerList[namesHolder.ActivePlayerList.Count - 1].GetComponent<Player>().PlayerNr) / 4;  
            if (val <= 1) val = 2;
            namesHolder.ActivePlayerList[namesHolder.ActivePlayerList.Count - 1].GetComponent<Player>().ActivateBonus(val);
        }
        else if(NewPlayerOption.value == 1)
        {
            graph.CreateSpaceForStrongPlayer();
        }
        
        if (AddTurns)
        {
            int AdditionalTurns;
            if (namesHolder.GetTurnLimit() - turnsCounter <= 7)
            {
                AdditionalTurns = 7;
            }
            else if (namesHolder.GetTurnLimit() - turnsCounter <= 14)
            {
                AdditionalTurns = 3;
            }
            else
            {
                AdditionalTurns = 0;
            }
            namesHolder.SetTurnLimit(namesHolder.GetTurnLimit() + AdditionalTurns);
            maxTurns.text = "/" + (namesHolder.GetTurnLimit());
        }

        ResetPlanetAvailability();
    }

    public void AddNewPlayer(string s, bool bot, int method)
    {

        string str = s;
        if (str != "")
            namesHolder.Add(str, bot, Strategy.casual);
        else
            namesHolder.Add("Unknown", bot, Strategy.casual);

        namesHolder.AddPlayer(PlayersCount, bot, Strategy.casual);
        PlayersCount = namesHolder.PlayerNumber();

        if (method == 0)
        {
            GameObject tmp = graph.CreateSpaceForNewPlayer();
            namesHolder.SetNewPlayerPosition(tmp);

            int val = MeanPlayersWinPoints(namesHolder.ActivePlayerList[namesHolder.ActivePlayerList.Count - 1].GetComponent<Player>().PlayerNr) / 4;
            if (val <= 1) val = 2;
            namesHolder.ActivePlayerList[namesHolder.ActivePlayerList.Count - 1].GetComponent<Player>().ActivateBonus(val);
        }
        else if (method == 1)
        {
            graph.CreateSpaceForStrongPlayer();
        }

        if (AddTurns)
        {
            int AdditionalTurns;
            if (namesHolder.GetTurnLimit() - turnsCounter <= 7)
            {
                AdditionalTurns = 7;
            }
            else if (namesHolder.GetTurnLimit() - turnsCounter <= 14)
            {
                AdditionalTurns = 3;
            }
            else
            {
                AdditionalTurns = 0;
            }
            namesHolder.SetTurnLimit(namesHolder.GetTurnLimit() + AdditionalTurns);
            maxTurns.text = "/" + (namesHolder.GetTurnLimit());
        }

        ResetPlanetAvailability();
    }

    public void AddNewPlayerWithHighLevel(GameObject item)
    {
        bool bot = false;
        string str = item.GetComponentInChildren<InputField>().text;
        if (str != "")
            namesHolder.Add(str,false, Strategy.casual);
        else
            namesHolder.Add("Unknown",false, Strategy.casual);
        namesHolder.AddPlayer(PlayersCount,bot, Strategy.casual);
        PlayersCount = namesHolder.PlayerNumber();         // gracz dodany

        graph.CreateSpaceForStrongPlayer();
        
        if (AddTurns)
        {
            int AdditionalTurns;
            if (namesHolder.GetTurnLimit() - turnsCounter <= 7)
            {
                AdditionalTurns = 7;
            }
            else if (namesHolder.GetTurnLimit() - turnsCounter <= 14)
            {
                AdditionalTurns = 3;
            }
            else
            {
                AdditionalTurns = 0;
            }
            namesHolder.SetTurnLimit(namesHolder.GetTurnLimit() + AdditionalTurns);
            maxTurns.text = "/" + (namesHolder.GetTurnLimit());
        }

        ResetPlanetAvailability();
    }
    private void PawnMove(GameObject pawn, GameObject planet)
    {
        if (pawn.GetComponent<Pawn>().GetActionPoints() > 0)
        {
            if (pawn.GetComponent<Pawn>().ReturnCurrentPlanet() != planet
                && graph.CheckConnection(pawn.GetComponent<Pawn>().ReturnCurrentPlanet(), planet))
            {
                ExplorePlanet(planet);

                if (planet.GetComponent<Location>().IsPawnHere())
                {
                    if (pawn != planet.GetComponent<Location>().GetPawn() && 
                        planet.GetComponent<Location>().GetPawn().GetComponent<Pawn>().Player.GetComponent<Player>().PlayerNr != pawn.GetComponent<Pawn>().Player.GetComponent<Player>().PlayerNr)
                    {
                        if (!currentPlayer.Bot)
                            StartCoroutine(StartBattle(pawn.GetComponent<Pawn>(), planet.GetComponent<Location>().GetPawn(), currentPlayer.Bot));
                        else
                        {
                            battle.Battle(pawn.GetComponent<Pawn>(), planet.GetComponent<Location>().GetPawn(), currentPlayer.Bot);
                        }
                    }
                }
                
                // Ustaw gracza na nowej planecie
                currentPlayer.GetComponent<Player>().SetPawnOnPlanet(planet);

                // zmniejsz pulę punktów zadań gracza
                pawn.GetComponent<Pawn>().ChangeDistanceOffset(planet.GetComponent<Location>().ReturnPawnCount());
                
                pawn.GetComponent<Pawn>().DecreaseActionPoints();
                UpdateLogs();
            }
        }
    } // wywołaj przemieszczenie pionka

    IEnumerator StartBattle(Pawn p1,Pawn p2, bool bot)
    {
        yield return new WaitForSeconds(1f);
        while (p1.CheckIfPawnMove())
        {
            yield return null;
        }
        //Pause = true;
        battle.Battle(p1,p2, bot);
    }
    // Zmiana tekstów na interfejsie gry
    public void TextChange(Text txt, string s)
    {
        txt.text = s;
    } // zmiana tekstu


    void CheckForNewPlayer()
    {
        foreach (PlayerPanel.PlayerData item in namesHolder.ReturnLaterPlayersList())
        {
            if (item.ActivationTurn == turnsCounter)
            {
                AddNewPlayer(item.name,true, item.Method);
            }
        }
    }

    bool EndTurnPause;
    public void EndTurn()
    {
        EndTurnPause = true;
        trybuild = true;
        tryMove = true;
        tryFight = true;
        ForceResearch = false;
        // jeśli ostatni gracz kończy turę to należy zwiększyć licznik tur
        if (namesHolder.CheckForLastPlayer(currentPlayer.gameObject))
        {
            turnsCounter++;
            if (turnsCounter == namesHolder.GetTurnLimit()+1)
            {
                EndGame();
            }
            else
            {
                // dodanie złota dla graczy
                AddGoldForPlayers();
            }

            if(!Ended)
            CheckForNewPlayer();
        }
        // należy zaznaczyć, że obecny gracz już nie jest aktywny
        currentPlayer.GetComponent<Player>().IsActive = false;
        ChangeCurrentPlayer();
        UpdateLogs();
        EndTurnPause = false;
    }// Funkcja jest odpowiedzialna za kończenie tury [BUTTON]
    public void ChangeCurrentPlayer()
    {
        // Zmiana gracza 
        currentPlayer = namesHolder.ReturnNextPlayer(currentPlayer.gameObject).GetComponent<Player>();

        //if (currentPlayer.Bot) StandardInterface.SetActive(false);
        //else StandardInterface.SetActive(true);


        currentPlayer.GetComponent<Player>().IsActive = true;
        Cam.GetComponent<CameraMovement>().ChangePosition(currentPlayer.GetComponent<Player>().ReturnPawn().transform);
        currentPlayer.GetComponent<Player>().RestartPoints();

        setKurtyna();

        ResetPlanetAvailability();
        UnselectSelectedObject();
        cityIdx = 0;

        if (currentPlayer.GetComponent<Player>().Bonus)
        {
            PlusIcon.SetActive(true);
            PlusIcon.GetComponentInChildren<Text>().text = "" + currentPlayer.GetComponent<Player>().BonusTime;
        }
        else
            PlusIcon.SetActive(false);
    } // zmiana gracza

    void setKurtyna()
    {
        if (currentPlayer.GetComponent<Player>().Bot)
        {
            kurtyna.SetActive(true);
        }
        else
        {
            kurtyna.SetActive(false);
        }
    }

    void ResetPlanetAvailability()
    {
        graph.HideAllPlanetVisibility();
        graph.HideAllSectorVisibility();
        graph.HideAllConnectionVisibility();
        foreach (GameObject item in currentPlayer.GetComponent<Player>().PlanetList)
        {
            item.GetComponent<Planet>()?.ShowPlanet();
        }
        foreach (SpaceSystem item in currentPlayer.GetComponent<Player>().SystemList)
        {
            item.gameObject.SetActive(true);
        }
        foreach (Connection item in currentPlayer.GetComponent<Player>().ConnectionList)
        {
            item.ActivateConnection(true);
        }
    }
    public void UpdateLogs()
    {
        CalculateWinPoints();
        // zmiana tekstów: licznika tur, złota gracza,oraz wpis do Logów gry.
        TextChange(turns, "" + turnsCounter);
        TextChange(Gold, "" + currentPlayer.GetComponent<Player>().goldAmount);
        TextChange(PlayerName, namesHolder.GetPlayerName(currentPlayer.gameObject));
        TextChange(QuestPoints, ""+currentPlayer.GetComponent<Player>().QuestPoints);
        TextChange(WinPoints, "" + currentPlayer.GetComponent<Player>().WinPoints);
        // UpdateLog(currentPlayer.name + " end his turn");
    }
    public void ShowMessage(float time, string txt)
    {
        StopAllCoroutines();
        StartCoroutine(Message(time,txt));
    }

    IEnumerator Message(float time,string txt)
    {
        MessageBox.GetComponentInChildren<Text>().text = txt;
        MessageBox.SetActive(true);        
        yield return new WaitForSeconds(time);
        MessageBox.SetActive(false);
    }

    public int GoldBonusMultiplier = 1;
    public int AdditionalActionPointsBonus = 0;
    public int AdditionalMovementPointsBonus = 0;

    public void AddGoldForPlayers()
    {
        foreach (GameObject sector in graph.GetComponent<GraphContainer>().SpaceSystemList)
        {
            foreach (GameObject planet in sector.GetComponent<SpaceSystem>().PlanetsList)
            {
                if ((planet.GetComponent<Planet>()))
                {
                    int PlayerNr = planet.GetComponent<Planet>().OwnerNumber;
                    if (PlayerNr != -1)
                        if (namesHolder.GetComponent<NamesHolder>().ReturnPlayerOnIndex(PlayerNr).GetComponent<Player>().Bonus == true)
                        {
                            namesHolder.GetComponent<NamesHolder>().ReturnPlayerOnIndex(PlayerNr).GetComponent<Player>().goldAmount = (GoldBonusMultiplier * planet.GetComponent<Planet>().ReturnMoney());
                        }
                        else
                        {
                            namesHolder.GetComponent<NamesHolder>().ReturnPlayerOnIndex(PlayerNr).GetComponent<Player>().goldAmount = planet.GetComponent<Planet>().ReturnMoney();
                        }
                    
                }
            }       
        }
    }// Funkcja wywoływana gdy mija pełna tura, dla każdego gracza przyznawane jest złoto

    public void CalculateWinPoints()
    {
        foreach (var player in namesHolder.PlayerList)
        {
            player.GetComponent<Player>().WinPoints = 0;
        }

        foreach (GameObject sector in graph.GetComponent<GraphContainer>().SpaceSystemList)
        {
            foreach (GameObject planet in sector.GetComponent<SpaceSystem>().PlanetsList)
            {
                if ((planet.GetComponent<Planet>()))
                {
                    int PlayerNr = planet.GetComponent<Planet>().OwnerNumber;
                    if (PlayerNr != -1)
                    {
                        if (planet.GetComponent<MinePlanet>())
                        {
                            namesHolder.GetComponent<NamesHolder>().ReturnPlayerOnIndex(PlayerNr).GetComponent<Player>().WinPoints += planet.GetComponent<MinePlanet>().MineLevel+1;
                        }
                        else if (planet.GetComponent<MotherPlanet>())
                        {
                            namesHolder.GetComponent<NamesHolder>().ReturnPlayerOnIndex(PlayerNr).GetComponent<Player>().WinPoints += (planet.GetComponent<MotherPlanet>().civilizationLevel+1);
                        }
                        else if (planet.GetComponent<Planet>())
                        {
                            namesHolder.GetComponent<NamesHolder>().ReturnPlayerOnIndex(PlayerNr).GetComponent<Player>().WinPoints += 1;
                        }
                    }
                }
            }
        }
    }
    public int MedianaWinPoints()
    {
        CalculateWinPoints();
        List<int> power = new List<int>();
        foreach (var item in namesHolder.ActivePlayerList)
        {
            if(item.GetComponent<Player>().WinPoints != 0)
            power.Add(item.GetComponent<Player>().WinPoints);
        }

        int n = power.Count;

        do
        {
            for (int i = 0; i < n - 1; i++)
            {
                if (power[i] > power[i + 1])
                {
                    int tmp = power[i + 1];
                    power[i + 1] = power[i];
                    power[i] = tmp;
                }
            }
            n = n - 1;
        } while (n > 1);

        foreach (var item in power)
        {
            Debug.LogWarning("power: "+ item);
        }

        n = power.Count;
        if (n % 2 != 1)// liczba parzysta
        {
            Debug.LogWarning("NEW power (parzyste): " + (int)(n / 2) + "    "+ ((int)(n / 2) - 1));
            return (power[(n / 2)] + power[(n / 2)-1])/2;
        }
        else//nieparzysta
        {
            int a = n / 2;
            Debug.LogWarning("nieparz... powerwww: " +a);
            Debug.LogWarning("nieparz... power: " + power[a]); // bo od zera się liczy
            return power[a+1];
        }
    }

    public int MeanPlayersWinPoints(int playerNr = -1)// player which not should be used
    {
        CalculateWinPoints();
        int points = 0;
        foreach (var player in namesHolder.ActivePlayerList)
        {
            if(player.GetComponent<Player>().PlayerNr != playerNr)
            points += player.GetComponent<Player>().WinPoints;
        }
        int ans;

        if (playerNr == -1)
            ans = (points / (namesHolder.ActivePlayerList.Count - 1));
        else
        {
            ans = (points / (namesHolder.ActivePlayerList.Count - 2));
        }

        return ans;
    }

    public void PawnOnCamCenter()
    {
        Cam.GetComponent<CameraMovement>().ChangePosition(currentPlayer.GetComponent<Player>().ReturnPawn().transform);
    } 

    int cityIdx = 0;
    public void CityOnCamCenter()
    {
        if (currentPlayer.GetComponent<Player>().MotherPlanetList.Count <= 0)
        {
            ShowMessage(2f,"You do not have any city.");
        }
        else if (currentPlayer.GetComponent<Player>().MotherPlanetList[cityIdx] != null)
        {
            Cam.GetComponent<CameraMovement>().ChangePosition(currentPlayer.GetComponent<Player>().MotherPlanetList[cityIdx].transform.position);
        }
        
        cityIdx++;
        if (cityIdx >= currentPlayer.GetComponent<Player>().MotherPlanetList.Count)
        {
            cityIdx = 0;
        }
    } //Wyśrodkowaniu kamery na mieście gracza [BUTTON] 




    // --------------------------------------------
    // koniec gry - podsumowanie -----------------
    // ---------------------------------------------
    void EndGame()
    {
        kurtyna.SetActive(false);

        Ended = true;

        foreach (var item in namesHolder.PlayerList)
        {
            item.GetComponent<Player>().WinPoints += (int)(item.GetComponent<Player>().goldAmount/1000);
            item.GetComponent<Player>().WinPoints += (int)(item.GetComponent<Player>().PlanetList.Count / 20);
            item.GetComponent<Player>().WinPoints += (int)(item.GetComponent<Player>().SystemList.Count / 5);
            item.GetComponent<Player>().WinPoints += (int)(item.GetComponent<Player>().ReturnPawn().GetComponent<Pawn>().power);
        }
        ScrollRect rect = EndGamePanel.GetComponentInChildren<ScrollRect>();

        namesHolder.SortPlayer();
        int i = 1;

        foreach (GameObject Player  in namesHolder.PlayerList)
        {
            GameObject obj = Instantiate(EndPlayerPanel, rect.content.transform, false) as GameObject;
            rect.content.ForceUpdateRectTransforms();
            
            if (i == 1)
            {
                obj.GetComponent<Image>().color = new Color(255f, 240f, 100f,0f);
            }
            else if (i == 2)
            {
                obj.GetComponent<Image>().color = new Color(200, 200, 200, 0f);
            }
            else if (i == 3)
            {
                obj.GetComponent<Image>().color = new Color(120, 120, 120, 0f);
            }

            Text[] tmp = obj.GetComponentsInChildren<Text>();
            tmp[2].text = ""+i;
            tmp[0].text = namesHolder.GetPlayerNameByNumber(Player.GetComponent<Player>().PlayerNr);
            tmp[1].text = "" + Player.GetComponent<Player>().WinPoints;
            Image[] imgs = obj.GetComponentsInChildren<Image>();
            imgs[2].color = Player.GetComponent<Player>().PlayerColor(Player.GetComponent<Player>().PlayerNr);

            if (!namesHolder.ActivePlayerList.Contains(Player))
            {
                obj.GetComponent<EndGamePlayerPanel>().PlayerDeath(Player.GetComponent<Player>().DeathTurn);
                tmp[1].text = "" + 0;
            }

            i++;
        }
        kurtyna.SetActive(false);
        EndGamePanel.SetActive(true);
    }

    //-----------------------------------------------------------------------------------------------
    // ------------------------------------------- Zapis gry ------------------------------------
    //-----------------------------------------------------------------------------------------------
    //private Save CreateSaveGameObject()
    //{
    //    Save save = new Save();
    //    foreach (GameObject location in graph.GetComponent<GraphContainer>().PlanetsList)
    //    {
    //        save.LocationsPoseX.Add((int)location.transform.position.x);
    //        save.LocationsPoseY.Add((int)location.transform.position.y);
    //    }
    //    return save;
    //}

    //public void SaveGame()
    //{
    //    Save save = CreateSaveGameObject();

    //    BinaryFormatter bf = new BinaryFormatter();
    //    FileStream file = File.Create(Application.persistentDataPath + "/gamesave.save");
    //    bf.Serialize(file, save);
    //    file.Close();
    //    Debug.Log("GameSaved");
    //}
    ////-------------------------load game ----------------------------
    //public void LoadGame()
    //{
    //    if (File.Exists(Application.persistentDataPath + "/gamesave.save"))
    //    {
    //        // 1
    //        graph.GetComponent<GraphContainer>().ClearLists();

    //        // 2
    //        BinaryFormatter bf = new BinaryFormatter();
    //        FileStream file = File.Open(Application.persistentDataPath + "/gamesave.save", FileMode.Open);
    //        Save save = (Save)bf.Deserialize(file);
    //        file.Close();

    //        // 3
    //        graph.GetComponent<GraphContainer>().RebuildWorld(save);
    //        Debug.Log("Load Completed!");

    //    }
    //    else
    //    {
    //        Debug.Log("No game saved!");
    //    }
    //}
    //bool Waiting = false;
    bool trybuild = true;
    bool tryMove = true;
    bool tryUpgradeShip = false;
    bool tryFight = true;
    bool ShowBotsMoves = false;

    public void Botcontrol()
    {
        Debug.LogError("endless");

        if (ShowBotsMoves)
        {
            if (WaitUntilLastActionLasts())
            {
                return;
            }
        }
        

        if (currentPlayer.QuestPoints <= 0 && currentPlayer.ReturnPawn().GetComponent<Pawn>().GetActionPoints() <= 0)
        {
            EndTurn();
            return;
        }

        if (!trybuild && !tryMove )
        {
            EndTurn();
            return;
        }

        if (currentPlayer.Tactics == Strategy.builder || currentPlayer.Tactics == Strategy.explorer)
        {

            if (trybuild)
            {
                if (ForceResearch)
                {
                    AIResearch(currentPlayer.PlanetList[0]);
                    ForceResearch = false;

                    if (!StillCanBuild() || currentPlayer.QuestPoints <= 0)
                    { trybuild = false; }

                }
                else
                {
                    BuildMode();
                }   
            }

            if (!trybuild)
            {
                if (tryMove)
                {
                    GoTakeNewPlanet();
                }
            }


            if ((!StillCanBuild() || currentPlayer.QuestPoints <= 0) && !ForceResearch)
            {
                trybuild = false;
            }
        }
        else if (currentPlayer.Tactics == Strategy.casual)
        {

            if (trybuild)
            {
                if (ForceResearch)
                {
                    Debug.LogError("Force research");
                    AIResearch(currentPlayer.PlanetList[0]);
                    ForceResearch = false;

                    if (!StillCanBuild() || currentPlayer.QuestPoints <= 0)
                    { trybuild = false; }

                }
                else
                {
                    BuildMode();
                }
            }

            if (tryMove)
            {
                GoTakeNewPlanet();
            }



            if ((!StillCanBuild() || currentPlayer.QuestPoints <= 0 ) && !ForceResearch)
            {
                trybuild = false;
            }
        }
        else if (currentPlayer.Tactics == Strategy.warrior)
        {
            if (tryFight)
            {
                AttackEnemy();
            }

            Debug.LogError(tryFight);

            if (!tryFight)
            {
                if (trybuild)
                {
                    if (ForceResearch)
                    {
                        AIResearch(currentPlayer.PlanetList[0]);
                        ForceResearch = false;

                        if (!StillCanBuild() || currentPlayer.QuestPoints <= 0)
                        { trybuild = false; }
                    }
                    else
                    {
                        BuildMode();
                    }
                }

                if (tryMove)
                {
                    GoTakeNewPlanet();
                }
            }

            if ((!StillCanBuild() || currentPlayer.QuestPoints <= 0) && !ForceResearch)
            {
                trybuild = false;
            }
        }

        Debug.LogError(trybuild + " : " +tryMove);
        Debug.LogError("path :" + currentPlayer.ReturnPawn().GetComponent<Pawn>().TargetPath.Count);
    }

    void AttackEnemy()
    {
        if (currentPlayer.ReturnPawn().GetComponent<Pawn>().TargetPath.Count > 0)
        {// go to enemy
            List<GameObject> path = currentPlayer.ReturnPawn().GetComponent<Pawn>().TargetPath;
            // pionek z ostatniej planety
            Pawn pawn = path[path.Count - 1].GetComponent<Location>().GetPawn();
            
            if (pawn != null)
            {
                if (pawn.GetComponent<Pawn>().Player.GetComponent<Player>().PlayerNr != currentPlayer.PlayerNr)
                {// jeśli ten pionek to inny gracz
                    if (currentPlayer.ReturnPawn().GetComponent<Pawn>().GetActionPoints() > 0)
                    {// mam jakieś pkt ruchu
                        if (currentPlayer.ReturnPawn().GetComponent<Pawn>().TargetPath[0] != null)
                        {// a to nie wiem po co ten 'if'
                            PawnMove(currentPlayer.ReturnPawn(), currentPlayer.ReturnPawn().GetComponent<Pawn>().TargetPath[0]);
                            currentPlayer.ReturnPawn().GetComponent<Pawn>().TargetPath.RemoveAt(0);// podszedłem planetę dalej do celu
                            Debug.LogError("");
                        }
                    }
                    else
                    {
                        tryFight = false;
                    }
                }
            }
            else
            {// wyczyść ścieżkę, poszukam nowej
                currentPlayer.ReturnPawn().GetComponent<Pawn>().TargetPath.Clear();
            }
        }
        else
        {
            Location planet = LookForOponent();
            if (planet != null)
            {// enemy finded
                List<GameObject> tmp = FindTheWay(currentPlayer.ReturnPawn().GetComponent<Pawn>().ReturnCurrentPlanet().GetComponent<Location>(), planet);
                if (tmp == null)
                {
                    tryFight = false;
                    return;
                }
                if (tmp.Count <= 0)
                {
                    tryFight = false;
                    return;
                }
                else
                {
                    currentPlayer.ReturnPawn().GetComponent<Pawn>().TargetPath = tmp;
                }
            }
            else
            {
                tryFight = false;
            }
        }
    }

    Location LookForOponent()
    {
        foreach (var sys in currentPlayer.SystemList)
        {
            foreach (var planet in sys.PlanetsList)
            {
                Pawn pawn = planet.GetComponent<Location>().GetPawn();

                if (pawn != null)
                {
                    if(currentPlayer.PlayerNr != pawn.Player.GetComponent<Player>().PlayerNr)
                    {
                        return planet.GetComponent<Location>();
                    }
                }
            }
        }
        return null;
    }

    private bool WaitUntilLastActionLasts()
    {
        //in move 
        if (currentPlayer.ReturnPawn().GetComponent<Pawn>().CheckIfPawnMove())
        {
            return true;
        }
        //or in battle
        return false;

        // true- action lasts
    }
    bool ForceResearch = false;

    GameObject WhatToBuild()
    {
        foreach (GameObject planet in currentPlayer.PlanetList)
        {
            if (planet.GetComponent<MotherPlanet>())
            {
                if (planet.GetComponent<MotherPlanet>().ReturnCivilizationCost() != 0)
                {
                    if (planet.GetComponent<MotherPlanet>().ReturnCivilizationCost() <= currentPlayer.goldAmount)
                    {
                        return planet;
                    }
                }
            }
            else if (planet.GetComponent<MinePlanet>())
            {
                if (planet.GetComponent<MinePlanet>().ReturnUpgradeCost() != 0)
                {
                    if (planet.GetComponent<MinePlanet>().ReturnUpgradeCost() <= currentPlayer.goldAmount)
                    {
                        return planet;
                    }
                }
            }
        }

        if (CheckIfBotCanResearch())
        {
            Debug.LogError("try research");
            ForceResearch = true;
        }
        else
        {
            trybuild = false;
        }

        return null;
    }

    void BuildMode()
    {
        if (currentPlayer.QuestPoints <= 0 || !StillCanBuild())
        {
            trybuild = false;
            return;
        }

        GameObject planet = WhatToBuild();

        if (planet != null)
        {
            if (planet.GetComponent<MotherPlanet>())
            {
                if (planet.GetComponent<MotherPlanet>())
                {
                    int treshold1 = 0;
                    int treshold2 = 0;

                    if (currentPlayer.Tactics == Strategy.casual)
                    {
                        treshold1 = 4;
                        treshold2 = 8;
                    }
                    else if (currentPlayer.Tactics == Strategy.builder)
                    {
                        treshold1 = 7;
                        treshold2 = 9;
                    }
                    else if (currentPlayer.Tactics == Strategy.explorer)
                    {
                        treshold1 = 3;
                        treshold2 = 9;
                    }
                    else if (currentPlayer.Tactics == Strategy.warrior)
                    {
                        treshold1 = 3;
                        treshold2 = 6;

                        if (currentPlayer.ReturnPawn().GetComponent<Pawn>().power >= 5)
                        {
                            treshold1 = 5;
                            treshold2 = 10;
                        }
                    }

                    int x = Random.Range(1, 11);

                    if (x <= treshold1)// build, upgrade
                    {
                        AICivilUpgrade(planet);
                    }// build
                    else if(x> treshold1 && x <= treshold2)
                    {
                        // research, if not - build
                        if (CheckIfBotCanResearch())
                        {
                            AIResearch(planet);
                        }
                        else
                        {
                            AICivilUpgrade(planet);

                        }
                    }
                    else if(x > treshold2)
                    {
                        UpgradeSpaceShipBot();
                    }
                }
                //if (planet.GetComponent<MotherPlanet>().ReturnCivilizationCost() != 0)
                //{
                //    if (planet.GetComponent<MotherPlanet>().ReturnCivilizationCost() <= currentPlayer.goldAmount)
                //    {
                //        AICivilUpgrade(planet);
                //    }
                //}
            }
            else if (planet.GetComponent<MinePlanet>())
            {
                if (planet.GetComponent<MinePlanet>().ReturnUpgradeCost() != 0)
                {
                    if (planet.GetComponent<MinePlanet>().ReturnUpgradeCost() <= currentPlayer.goldAmount)
                    {
                        AIMineUpgrade(planet);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("nothing to build");
            //if (CheckIfBotCanResearch())
            //{
            //    Debug.LogError("try research");
            //    ForceResearch = true;
            //}
            //trybuild = false;
        }
    }

    // ========== OLD ==========
    void TryToBuild()
    {
        foreach (GameObject planet in currentPlayer.PlanetList)
        {
            if (currentPlayer.QuestPoints <= 0 || !StillCanBuild())
            {
                trybuild = false;
                return;
            }
            else
            {
                Debug.LogError("quest points " + currentPlayer.QuestPoints +" ,can build "+ StillCanBuild());
            }

            if (planet.GetComponent<MotherPlanet>() || planet.GetComponent<MinePlanet>())
            {
                if (planet.GetComponent<Planet>().OwnerNumber == currentPlayer.PlayerNr)
                {
                    if (planet.GetComponent<MotherPlanet>())
                    {
                        int x = Random.Range(0, 11);

                        int treshold = 0;
                        if (currentPlayer.Tactics == Strategy.explorer)treshold = 6;
                        else treshold = -1; // builder - always build, and then build


                        if (x < treshold)
                        {// research
                            if (CheckIfBotCanResearch())
                            {
                                AIResearch(planet);
                                Debug.LogError("3");
                            }
                            else
                            {
                                Debug.LogError("2");
                                AICivilUpgrade(planet);
                                Debug.LogError("2");
                            }
                        }// build
                        else
                        {
                            AICivilUpgrade(planet);
                            Debug.LogError("4");
                        }
                    }
                    else if (planet.GetComponent<MinePlanet>())
                    {
                        AIMineUpgrade(planet);
                        Debug.LogError("1");
                    }
                }
            }
        }
    }

    public void AIMineUpgrade(GameObject planet)
    {
        if (planet != null)
        {
            SelectedObject = planet;
        }
        MineUpgrade();
    }

    public void AICivilUpgrade(GameObject planet)
    {
        if (planet != null)
        {
            SelectedObject = planet;
        }
        CivilUpgrade();
    }

    public void AIResearch(GameObject planet)
    {
        if (planet != null)
        {
            SelectedObject = planet;
        }
        Research();
    }
    
     bool StillCanBuild()
     {
        foreach (GameObject planet in currentPlayer.PlanetList)
        {
            if (planet.GetComponent<MotherPlanet>())
            {
                if (planet.GetComponent<MotherPlanet>().ReturnCivilizationCost() != 0)
                {
                    if (planet.GetComponent<MotherPlanet>().ReturnCivilizationCost() <= currentPlayer.goldAmount)
                    {
                        return true;
                    }
                }
            }
            else if (planet.GetComponent<MinePlanet>())
            {
                if (planet.GetComponent<MinePlanet>().ReturnUpgradeCost() != 0)
                {
                    if (planet.GetComponent<MinePlanet>().ReturnUpgradeCost() <= currentPlayer.goldAmount)
                    {
                        return true;
                    }
                }
            }
        }

        if (CheckIfBotCanResearch())
        {
            ForceResearch = true;
        }
        else
        {
            trybuild = false;
        }

        return false;
     }

    void GoTakeNewPlanet()
    {
        if (currentPlayer.ReturnPawn().GetComponent<Pawn>().GetActionPoints() > 0)
        {
            if (currentPlayer.ReturnPawn().GetComponent<Pawn>().TargetPath.Count <= 0) // nie mam celu trasy
            {
                GameObject nextPlanet = ReturnFirstNotMyPlanetInSector(true);
                
                if (nextPlanet != null)// przemieszczanie się w obrębie sektoru
                {
                    Debug.LogError(nextPlanet.name);
                    Debug.LogError("got planet in sector to go");
                    if (nextPlanet != currentPlayer.ReturnPawn().GetComponent<Pawn>().ReturnCurrentPlanet())// jesli idę na inną planetę z tego samego sektora
                    {
                        PawnMove(currentPlayer.ReturnPawn(), nextPlanet); // odejmuję pkt akcji pionka
                        if (currentPlayer.QuestPoints > 0)
                        {
                            if (nextPlanet.GetComponent<Planet>().OwnerNumber != currentPlayer.PlayerNr)//zajmij tę planętę jeśli trzeba
                            {
                                nextPlanet.GetComponent<Planet>().ChangeOwner(currentPlayer.PlayerNr);
                                currentPlayer.QuestPoints--;
                            }
                        }
                        else
                        {
                            tryMove = false;
                            return;
                        }
                    }
                    else // chce iść na planetę na ktorej jestem
                    {
                        if (currentPlayer.QuestPoints > 0)
                        {
                            if (nextPlanet.GetComponent<Planet>().OwnerNumber != currentPlayer.PlayerNr)//zajmij tę planętę jeśli trzeba
                            {
                                nextPlanet.GetComponent<Planet>().ChangeOwner(currentPlayer.PlayerNr);
                                currentPlayer.QuestPoints--;
                            }
                        }
                        else
                        {
                            tryMove = false;
                            return;
                        }
                    }
                }
                else // wszystkie planety w tym sektorze są moje, muszę iść dalej - wyznacz ścieżkę
                {
                    if (currentPlayer.ReturnPawn().GetComponent<Pawn>().GetActionPoints() <= 0)
                    {
                        tryMove = false;
                        return;
                    }

                    nextPlanet = ReturnFirstNotMyPlanetInSector(false);
                    if (nextPlanet != null)
                    {
                        //if (currentPlayer.MotherPlanetList.Count <= 0)
                        //{
                        //    tryMove
                        //    return;
                        //}
                        Debug.LogError(nextPlanet.name);
                        List<GameObject> tmp = FindTheWay(currentPlayer.ReturnPawn().GetComponent<Pawn>().ReturnCurrentPlanet().GetComponent<Location>(), nextPlanet.GetComponent<Location>());

                        if (tmp.Count <= 0)
                        {
                            tryMove = false;
                            return;
                        }
                        else
                        currentPlayer.ReturnPawn().GetComponent<Pawn>().TargetPath =tmp;
                    }  
                    else// czyli nie mam gdzieiść
                    {
                        tryMove = false;
                        return;
                    }
                }
            }
            else // idź wzdłuż ścieżki
            {
                if (currentPlayer.ReturnPawn().GetComponent<Pawn>().GetActionPoints() > 0)
                {
                    if (currentPlayer.ReturnPawn().GetComponent<Pawn>().TargetPath[0] != null)
                    {
                        PawnMove(currentPlayer.ReturnPawn(), currentPlayer.ReturnPawn().GetComponent<Pawn>().TargetPath[0]);
                        currentPlayer.ReturnPawn().GetComponent<Pawn>().TargetPath.RemoveAt(0);
                        Debug.LogError("");
                    }
                    
                }
                else
                {
                    tryMove = false;
                    return;
                }
            }


        }
        else
        {
            tryMove = false;
        }

    }

    GameObject ReturnFirstNotMyPlanetInSector(bool PawnSector)
    {
        SpaceSystem sector;
        if (PawnSector)
        {
            sector = currentPlayer.ReturnPawn().GetComponent<Pawn>().ReturnCurrentPlanet().GetComponent<Location>().ReturnSector();
            foreach (GameObject item in sector.PlanetsList)
            {
                if (item.GetComponent<Planet>())
                {
                    if (item.GetComponent<Planet>().OwnerNumber != currentPlayer.PlayerNr)
                    {
                        // w sektorze jest nie moja plnaeta
                        return item;
                    }
                }
            }
        }
        else
        {
            foreach (var sectorSys in currentPlayer.SystemList)
            {
                foreach (GameObject item in sectorSys.PlanetsList)
                {
                    if (item.GetComponent<Planet>())
                    {
                        if (item.GetComponent<Planet>().OwnerNumber != currentPlayer.PlayerNr)
                        {
                            // w sektorze jest nie moja plnaeta
                            return item;
                        }
                    }
                }
            }
        }
        // nie ma w tym sektorze już nic fajnego
        return null;
    }

    List<GameObject> FindTheWay(Location start, Location end)
    {
        //preparing...
        Debug.LogError("checking....");
        foreach (var item in graph.SpaceSystemList)
        {
            item.GetComponent<SpaceSystem>().Last = null;
        }

        //znajdź wszystkie połączenia
        List<SpaceSystem> Done = new List<SpaceSystem>();
        List<GameObject> DoneConn = new List<GameObject>();
        start.ReturnSector().GetComponent<SpaceSystem>().CheckPath(null, end.ReturnSector().GetComponent<SpaceSystem>(), Done, DoneConn, currentPlayer);

        if (end.ReturnSector().GetComponent<SpaceSystem>().Last == null) // nie znaleziono ścieżki
        {// nie powinno się tak zdażyć, zawsze szukamy planety z połączenia, jest to dodatkowe zabezpieczenie
            return null;
        }

        //Connection conn = end.ReturnSector().GetComponent<SpaceSystem>().Last;
        SpaceSystem last = end.ReturnSector().GetComponent<SpaceSystem>();

        List<GameObject> path = new List<GameObject>();


        // znajdź połączenie end i last
        SpaceSystem sector = end.ReturnSector();
        while (sector != null)
        {
            if (sector.Last != null)
            {
                GameObject conn = graph.ReturnSectorsConnection(sector, sector.Last);

                if (conn == null) Debug.LogError("2");
                if (conn.GetComponent<Connection>().Planet1)
                {
                    Debug.LogError("1");
                }

                if (sector.PlanetsList.Contains(conn.GetComponent<Connection>().Planet1))
                {
                    path.Add(conn.GetComponent<Connection>().Planet1);
                    path.Add(conn.GetComponent<Connection>().Planet2);
                }
                else
                {
                    path.Add(conn.GetComponent<Connection>().Planet2);
                    path.Add(conn.GetComponent<Connection>().Planet1);
                }
            }

            sector = sector.Last;
        }
        
        path.Reverse();
        path.Add(end.gameObject);
        return path;
        //StartCoroutine(GoThroughPath(path));
    }

    bool CheckIfBotCanResearch()
    {
        bool CityLevel = false;
        bool Gold = false;

        foreach (MotherPlanet item in currentPlayer.MotherPlanetList)
        {
            if (item.civilizationLevel >= 2)
            {
                CityLevel = true;
                break;
            }
        }

        if (currentPlayer.goldAmount >= 500)
        {
            Gold = true;
        }

        return Gold && CityLevel;
    }

    IEnumerator GoThroughPath(List<GameObject> path)
    {
        foreach (var item in path)
        {
            currentPlayer.ReturnPawn().GetComponent<Pawn>().SetCurrentPlanet(item);
            yield return new WaitForSeconds(0.1f);
            while (currentPlayer.ReturnPawn().GetComponent<Pawn>().CheckIfPawnMove())
            {
                yield return null;
            }
        }
    }
}