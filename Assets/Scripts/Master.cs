using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class Master : MonoBehaviour
{
    [SerializeField] AudioSource cash_sound;

    // Elementy interfejsu
    // Informacja o złocie, turach, oraz nazwa aktualnego gracza 
    // i jego punkty zadań
    public Text QuestPoints;
    public Text Gold;
    public Text turns;
    public Text PlayerName;
    // dziennik logów
    public Text log;
    private string logText;
    // licznik tur
    float turnsCounter = 1;
    // Panele planet
    [SerializeField] private GameObject CityPanel;
    [SerializeField] private GameObject MinePanel;
    [SerializeField] private GameObject TakePlanetPanel;
    // Obecny gracz,Obecnie zaznaczony obiekt
    private GameObject currentPlayer;
    private GameObject SelectedObject;
    // liczba graczy, tabela indeksów planet startowych
    private int PlayersCount;
    int[] tab;

    // wskaźniki do obiektów typu:kamera, 
    [SerializeField] private GameObject Cam;
    //Pojemnik na graf (mapę) -- klasa
    [SerializeField] private GraphContainer graph;
    // wyzwalacz walki -- klasa
    [SerializeField] private BattleHandler battle;
    // Pojemnik na graczy oraz ich nazwy -- klasa
    [SerializeField] private NamesHolder namesHolder;



    void Start()
    {
        // pobierz liczbę graczy
        PlayersCount = namesHolder.PlayerNumber();

        // Utworzenie mapy w zależności od liczby graczy, zwrócenie indeksów planet-matek
        int[] tab = graph.Initialize(PlayersCount);

        namesHolder.Test();
        // Utworzenie graczy
        namesHolder.InitPlayers();


        //int[] tab = {0,3,5,7,9,11,13,15};   -------// ustawienie wstępnych planet dla graczy

        // ustawaimy je na planetach z indeksami z tablicy "tab" - TESTOWO
        namesHolder.SetStartPositions(graph, tab);

        //------------------------------------------------------------------
        // Inicjalizacja gry, czyli:
        // ustawienie pierwszego gracza do rozegrania jego tury
        currentPlayer = namesHolder.ReturnNextPlayer();
        // ustawiamy ze gracz jest aktywny
        currentPlayer.GetComponent<Player>().IsActive = true;

        //Ustawiamy kamerę na pozycji pionka
        Cam.GetComponent<CameraMovement>().ChangePos(currentPlayer.GetComponent<Player>().ReturnPawn().transform);

        // Ustaw teksty jak nazwa gracza,jego złoto, licznik tur
        UpdateLogs();
    } // wykonuje się raz, zainicjowanie tworzenia mapy, tworzenie graczy, rozmieszczenie graczy, ustawienie aktualnego gracza
    void Update()
    {
        HandleInputs();
    } // wywołuje f. HandleInputs co chwilę czasową
    void FixedUpdate()
    {

    }// f. pusta, wykonuje się co 1 klatkę

    void HandleInputs()
    {
        if (Input.GetMouseButtonDown(1) && SelectedObject != null)
        {
            if (SelectedObject.tag == "Pawn")
            {
                SelectedObject.GetComponent<Pawn>().TurnLightOff();
                SelectedObject = null;
            }
            else if (SelectedObject.tag == "Planet")
            {
                SelectedObject.GetComponent<Location>().TurnLightOff();
                SelectedObject = null;
            }
        }



        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            // hit to obiekt który został kliknięty
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);


            // najpierw sprawdz czy klikam jakis obiekt
            if (hit.collider != null)
            {
                //-------------------------------------------------------
                // sprawdz czy wybrany (kliknięty) obiekt to pionek
                //-------------------------------------------------------

                if (hit.collider.gameObject.tag == "Pawn")
                {
                    if (SelectedObject == null)
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
                            SelectedObject.GetComponent<Location>().TurnLightOff();
                            SelectedObject = hit.collider.gameObject;
                            SelectedObject.GetComponent<Pawn>().TurnLightOn();
                        }
                        else if (SelectedObject.tag == "Pawn")
                        {
                            SelectedObject.GetComponent<Pawn>().TurnLightOff();
                            SelectedObject = hit.collider.gameObject;
                            SelectedObject.GetComponent<Pawn>().TurnLightOn();
                        }
                    }

                }
                // sprawdz czy klinkiete jest planetą
                else if (hit.collider.gameObject.tag == "Planet")
                {
                    // klikam na planetę i .......
                    if (SelectedObject == null) // do tej pory nie miałem nic zaznaczone 
                    {
                        SelectedObject = hit.collider.gameObject;
                        SelectedObject.GetComponent<Location>().TurnLightOn();
                    }
                    else if (hit.collider.gameObject == SelectedObject) // mma już zaznaczoną tę planetę
                    {
                        SelectedObject.GetComponent<Location>().TurnLightOff();
                        SelectedObject = null;
                    }
                    else if (hit.collider.gameObject != SelectedObject) // mam już zaznaczony inny obiekt
                    {
                        // inny zaznaczony obiekt jest planetą
                        if (SelectedObject.tag == "Planet")
                        {
                            SelectedObject.GetComponent<Location>().TurnLightOff();
                            SelectedObject = hit.collider.gameObject;
                            SelectedObject.GetComponent<Location>().TurnLightOn();
                        }
                        // inny zaznaczony obiekt jest pionkiem (mam zaznaczony pionek i klikam na planetę)
                        else if (SelectedObject.tag == "Pawn" /* && pawn jest aktualnego gracza */)
                        {
                            PawnMove(SelectedObject, hit.collider.gameObject);

                        }
                    }

                    //------------------------------------
                    // tutaj kod obsługi wybranej planety
                    //------------------------------------

                    // jeśli zaznaczony obiekt nie jest NULL
                    if (SelectedObject != null)
                    {
                        // jeśli obiekt należy do mnie
                        if (currentPlayer.GetComponent<Player>().PlayerNr1 == SelectedObject.GetComponent<Location>().OwnerNumber)
                        {
                            // otwórz panel miasta
                            if (SelectedObject.GetComponent<Location>().Type == "City")
                            {
                                // ustaw odpowiednio tekst na panelu
                                RefreshCityPanel();
                                //Text[] txt = CityPanel.GetComponentsInChildren<Text>();
                                //txt[2].text = "Civilization Level: " + SelectedObject.GetComponent<MotherPlanet>().civilizationLevel;
                                //txt[3].text =""+ SelectedObject.GetComponent<MotherPlanet>().ReturnCivilizationCost();  

                                // otwórz panel
                                gameObject.GetComponent<ButtonAction>().TooglePanel(CityPanel);
                            }
                            else if (SelectedObject.GetComponent<Location>().Type == "Mine")
                            {
                                RefreshMinePanel();
                                gameObject.GetComponent<ButtonAction>().TooglePanel(MinePanel);
                            }
                        }// jeśli nie należy do mnie, ale za to jest na nim mój pionek
                        else if (currentPlayer.GetComponent<Player>().ReturnPawn() == SelectedObject.GetComponent<Location>().GetPawnObject())
                        {
                            // wyświetl panel PRZEJMOWANIA PLANETY
                            gameObject.GetComponent<ButtonAction>().TooglePanel(TakePlanetPanel);
                        }

                    }
                }
            }
        }
    } // obsługa sterowania myszą, duża część logiki sterowania graczem
    
    public void TakeOverPlanet()
    {
        if (currentPlayer.GetComponent<Player>().QuestPoints > 0
            && currentPlayer.GetComponent<Player>().ReturnPawn() == SelectedObject.GetComponent<Location>().GetPawnObject())
        {
            SelectedObject.GetComponent<Location>().ChangeOwner(currentPlayer.GetComponent<Player>().PlayerNr1);

            currentPlayer.GetComponent<Player>().QuestPoints--;
            UpdateLogs();
        }

    }// przejmij planetę, funkcja dla przycisku z panelu TAKEOVERPLANET PANEL [BUTTON]
    public void RefreshMinePanel()
    {
        Text[] txt = MinePanel.GetComponentsInChildren<Text>();
        txt[2].text = "Mine Level: " + SelectedObject.GetComponent<MinePlanet>().MineLevel;
        txt[3].text = "" + SelectedObject.GetComponent<MinePlanet>().ReturnUpgradeCost();
    }// odświeżanie panelu kopalni
    public void RefreshCityPanel()
    {
        Text[] txt = CityPanel.GetComponentsInChildren<Text>();
        txt[2].text = "Civilization Level: " + SelectedObject.GetComponent<MotherPlanet>().civilizationLevel;
        txt[3].text = "" + SelectedObject.GetComponent<MotherPlanet>().ReturnCivilizationCost();
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
            Debug.Log("Ulepszono maisto !");
        }
        else
        {
            Debug.Log("Za mało złota lub poziom miasta jest maksymalny!");
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
            Debug.Log("Ulepszono kopalnię !");
        }
        else
        {
            Debug.Log("Za mało złota lub poziom kopalni jest maksymalny!");
        }
    }  // analogicznie dla MINE PANEL [BUTTON]
    public void Research() 
    {
        if (currentPlayer.GetComponent<Player>().QuestPoints > 0)
        {
            graph.AddMap(SelectedObject);
            // SelectedObject
            currentPlayer.GetComponent<Player>().QuestPoints--;
            UpdateLogs();

        }
    }// dobudowanie mapy po kliknięciu przycisku RESEARCH [BUTTON]

    private void PawnMove(GameObject pawn, GameObject planet)
    {
        // Przemieszczanie się po planszy
        // jeśli mam punnkty zadań,
        if (currentPlayer.GetComponent<Player>().QuestPoints > 0)
        {
            //jeśli planeta na którą klikam jest różna od tej na której jestem AND sprawdzenie czy planety są połączone
            if (pawn.GetComponent<Pawn>().ReturnCurrentPlanet() != planet
                && graph.CheckConnection(pawn.GetComponent<Pawn>().ReturnCurrentPlanet(), planet))
            {
                // sprawdź cczy na planecie na którą klikam jest już pionek -------------------nie sprawdzam tego jeszcze (czy jest on innego gracza)
                if (planet.GetComponent<Location>().IsPawnHere())
                {
                    // rozpoczynam walkę pomiedzy aktualnym graczem, a graczem ktory jest na planecie
                    battle.Battle(pawn.GetComponent<Pawn>(), planet.GetComponent<Location>().GetPawn());
                    planet.GetComponent<Location>().GetPawn().Death();
                }

                // Ustaw gracza na nowej planecie 
                currentPlayer.GetComponent<Player>().SetPawnOnPlanet(planet);
                
                // zmniejsz pulę punktów zadań gracza
                currentPlayer.GetComponent<Player>().QuestPoints -= 1;
                UpdateLogs();
            }
        }
    } // wywołaj przemieszczenie pionka

    // Zmiana tekstów na interfejsie gry
    public void TextChange(Text txt, string s)
    {
        txt.text = s;
    } // zmiana tekstu
    public void UpdateLog(string s)
    {
        logText = s + " /n " + logText;
        log.text = logText;
    } // aktualizacja tekstu w oknie komunikatów, aktualnie wyłączona opcja

    public void EndTurn()
    {
        // jeśli ostatni gracz kończy turę to należy zwiększyć licznik tur
        if (namesHolder.CheckForLastPlayer(currentPlayer))
        {
            turnsCounter++;
            // dodanie złota dla graczy
            AddGoldForPlayers();
        }
        // należy zaznaczyć, że obecny gracz już nie jest aktywny
        currentPlayer.GetComponent<Player>().IsActive = false;
        ChangeCurrentPlayer();
        UpdateLogs();
    }// Funkcja jest odpowiedzialna za kończenie tury [BUTTON]
    public void ChangeCurrentPlayer()
    {
        // Zmiana gracza 
        if (currentPlayer.GetComponent<Player>().IsActive == false)
        {
            currentPlayer = namesHolder.ReturnNextPlayer(currentPlayer);
            currentPlayer.GetComponent<Player>().IsActive = true;
            Cam.GetComponent<CameraMovement>().ChangePos(currentPlayer.GetComponent<Player>().ReturnPawn().transform);
            currentPlayer.GetComponent<Player>().RestartPoints();
        }
    } // zmiana gracza
    public void UpdateLogs()
    {
        // zmiana tekstów: licznika tur, złota gracza,oraz wpis do Logów gry.
        TextChange(turns, "Turn: " + turnsCounter);
        TextChange(Gold, "Gold: " + currentPlayer.GetComponent<Player>().goldAmount);
        TextChange(PlayerName, namesHolder.GetPlayerName(currentPlayer));
        TextChange(QuestPoints, ""+currentPlayer.GetComponent<Player>().QuestPoints);
        // UpdateLog(currentPlayer.name + " end his turn");
    }
    public void AddGoldForPlayers()
    {
        foreach (GameObject item in graph.GetComponent<GraphContainer>().PlanetsList)
        {
            int PlayerNr = item.GetComponent<Location>().OwnerNumber;
            if (PlayerNr != -1)
            {
                namesHolder.GetComponent<NamesHolder>().ReturnPlayerOnIndex(PlayerNr).GetComponent<Player>().goldAmount = item.GetComponent<Location>().ReturnMoney();
            } 
        }
    }// Funkcja wywoływana gdy mija pełna tura, dla każdego gracza przyznawane jest złoto

    public void PawnOnCamCenter()
    {
        Cam.GetComponent<CameraMovement>().ChangePos(currentPlayer.GetComponent<Player>().ReturnPawn().transform);
    }   //Wyśrodkowaniu kamery na pionku gracza [BUTTON]
    public void CityOnCamCenter()
    {
        Cam.GetComponent<CameraMovement>().ChangePos(currentPlayer.GetComponent<Player>().GetStartPlanet.transform);
    } //Wyśrodkowaniu kamery na mieście gracza [BUTTON] 







    //-----------------------------------------------------------------------------------------------
    // ------------------------------------------- Zapis gry ------------------------------------
    //-----------------------------------------------------------------------------------------------
    private Save CreateSaveGameObject()
    {
        Save save = new Save();
        foreach (GameObject location in graph.GetComponent<GraphContainer>().PlanetsList)
        {
            save.LocationsPoseX.Add((int)location.transform.position.x);
            save.LocationsPoseY.Add((int)location.transform.position.y);
        }
        return save;
    }

    public void SaveGame()
    {
        Save save = CreateSaveGameObject();

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/gamesave.save");
        bf.Serialize(file, save);
        file.Close();
        Debug.Log("GameSaved");
    }
    //-------------------------load game ----------------------------
    public void LoadGame()
    {
        if (File.Exists(Application.persistentDataPath + "/gamesave.save"))
        {
            // 1
            graph.GetComponent<GraphContainer>().ClearLists();

            // 2
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/gamesave.save", FileMode.Open);
            Save save = (Save)bf.Deserialize(file);
            file.Close();

            // 3
            graph.GetComponent<GraphContainer>().RebuildWorld(save);
            Debug.Log("Load Completed!");

        }
        else
        {
            Debug.Log("No game saved!");
        }
    }

}