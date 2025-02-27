using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MarketDeckController : MonoBehaviourPunCallbacks
{
    public static MarketDeckController instance;
    public int cardcount = 0;

    private void Awake()
    {
        instance = this;

    }

    public List<CardGlobalMarketScriptableObj> deckToUse = new List<CardGlobalMarketScriptableObj>();

    public List<CardGlobalMarketScriptableObj> activeCards = new List<CardGlobalMarketScriptableObj>();

    public List<CardGlobalMarketScriptableObj> usedCards = new List<CardGlobalMarketScriptableObj>();

    public List<CardGlobalMarketScriptableObj> tempDeck = new List<CardGlobalMarketScriptableObj>();

    //int iterations = 0;

    public MarketCard cardsToSpawns;

    public int FordrawCard = 1;
    public float waitBetweenDrawingCard = .3f;


    // Start is called before the first frame update
    void Start()
    {

        //photonView.RPC("setUpdeckToEveryone", RpcTarget.All);
        SetUpDeck();
        UIController.instance.drawButton.SetActive(false);
        UIController.instance.cardShow.enabled = false;
        UIController.instance.cancelButton.SetActive(false);
        UIController.instance.passButton.SetActive(false);
        UIController.instance.payButton.SetActive(false);
        UIController.instance.MarketDrawButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetUpDeck()
    {


        activeCards.Clear();
        tempDeck.AddRange(deckToUse);
        if (PhotonNetwork.IsMasterClient)
        {
            int iterations = 0;
            while (tempDeck.Count > 0 && iterations < 500)
            {
                int selected = Random.Range(0, tempDeck.Count);
                photonView.RPC("CreateMarketDeckStart", RpcTarget.All, selected);

                iterations++;
            }
        }

    }

    public void DrawCardToHand()
    {


        //SetUpDeck();
        Debug.Log("Active Card" + activeCards.Count);


        MarketCard newCard = Instantiate(cardsToSpawns, transform.position, transform.rotation);
        newCard.cardGMSO = activeCards[0];

        UIController.instance.payButton.SetActive(false);
        UIController.instance.drawButton.SetActive(false);
        UIController.instance.SellButton.SetActive(false);
        UIController.instance.MarketDrawButton.SetActive(false);
        UIController.instance.cancelButton.SetActive(true);
        photonView.RPC("ShowCardToAllPlayerRPC", RpcTarget.All);
        photonView.RPC("AddToUseCard", RpcTarget.All);

        

        photonView.RPC("whoCanSellandCancelMarket", RpcTarget.All);
        if (usedCards[cardcount - 1].destroy == true
                && (GameManager.instace.playerList[GameManager.instace.activePlayer].hasHome32
                || GameManager.instace.playerList[GameManager.instace.activePlayer].hasHome21
                || GameManager.instace.playerList[GameManager.instace.activePlayer].hasCondominium21
                || GameManager.instace.playerList[GameManager.instace.activePlayer].hascommercialBuilding
                || GameManager.instace.playerList[GameManager.instace.activePlayer].hasApartment))
        {
            UIController.instance.MarketPayButton.SetActive(true);
        }
        if(usedCards[cardcount -1].increaseIncome == true)
        {
            photonView.RPC("UpdateDealIncomeInList", RpcTarget.All);
            photonView.RPC("valueUpdate", RpcTarget.All);
            UIController.instance.cancelButton.SetActive(true);
        }
        Destroy(newCard.gameObject, 1);
    }

    public void SellCost()
    {
        if (usedCards[cardcount - 1].house3s2 == true && usedCards[cardcount - 1].house2s1 == true 
            && usedCards[cardcount - 1].Condominium == true && usedCards[cardcount - 1].CommercialBuilding == true 
            && usedCards[cardcount - 1].Apartment == true)
        {
            UIController.instance.SellListFromMarketCanvas.SetActive(true);
            UIController.instance.BlurBg.SetActive(true);

        }
        else if (usedCards[cardcount - 1].GoldCoins == true)
        {
            UIController.instance.GoldCoinsSell.SetActive(true);
            UIController.instance.cancelButton.SetActive(false);
            UIController.instance.BlurBg.SetActive(true);
        }
        
            

    }
    

    public void PayCost()
    {
        photonView.RPC("UpdateDealListRPC", RpcTarget.All);
        if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].money >= usedCards[cardcount - 1].Cost)
        {
            if (usedCards[cardcount - 1].destroy == true 
                && (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasHome32 
                || GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasHome21
                || GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasCondominium21
                || GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hascommercialBuilding
                || GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasApartment))
            {
                UIController.instance.MarketPayButton.SetActive(false);
                GameManager.instace.playerList[GameManager.instace.activePlayer].money = GameManager.instace.playerList[GameManager.instace.activePlayer].money - usedCards[cardcount - 1].Cost;
                photonView.RPC("UpdateMoney", RpcTarget.All, GameManager.instace.playerList[GameManager.instace.activePlayer].money, GameManager.instace.activePlayer);
                UIController.instance.drawButton.SetActive(false);
                UIController.instance.cardShow.enabled = false;
                UIController.instance.payButton.SetActive(false);
                UIController.instance.BigPayButton.SetActive(false);
                UIController.instance.SellButton.SetActive(false);
                UIController.instance.cancelButton.SetActive(false);
                UIController.instance.SmallPayButton.SetActive(false);
                UIController.instance.MarketPayButton.SetActive(false);
                UIController.instance.MarketSellButton.SetActive(false);
                UIController.instance.passButton.SetActive(true);

            }
            else
            {
                UIController.instance.drawButton.SetActive(false);
                UIController.instance.cardShow.enabled = false;
                UIController.instance.payButton.SetActive(false);
                UIController.instance.BigPayButton.SetActive(false);
                UIController.instance.SellButton.SetActive(false);
                UIController.instance.cancelButton.SetActive(false);
                UIController.instance.SmallPayButton.SetActive(false);
                UIController.instance.MarketPayButton.SetActive(false); ;
                UIController.instance.MarketSellButton.SetActive(false);
                UIController.instance.passButton.SetActive(true);
            }
     
        }
        else if (GameManager.instace.playerList[GameManager.instace.activePlayer].getmoney <= 0 && GameManager.instace.playerList[GameManager.instace.activePlayer].money < usedCards[cardcount - 1].Cost)
        {
            //lose
            GameManager.instace.playerList[GameManager.instace.activePlayer].playerType = GameManager.Entity.PlayerTypes.NO_PLAYER;
            GameManager.instace.playerInRoom = 0;
            for (int i = 0; i < GameManager.instace.playerList.Count; i++)
            {
                if (GameManager.instace.playerList[i].playerType == GameManager.Entity.PlayerTypes.NO_PLAYER)
                {
                    GameManager.instace.playerInRoom++;
                }

            }
            if (GameManager.instace.playerInRoom > 0)
            {
                GameManager.instace.state = GameManager.States.SWITCH_PLAYER;
            }
            UIController.instance.BlurBg.SetActive(true);
            UIController.instance.lostShow.SetActive(true);
        }
        else
        {
            UIController.instance.LoanCanvas.SetActive(true);
            UIController.instance.BlurBg.SetActive(true);
        }


        photonView.RPC("valueUpdate", RpcTarget.All);


    }
    private bool IsMyTurn()
    {
        // Replace with your logic. This could be checking against a player list, an ID, etc.
        return GameManager.instace.activePlayer == PhotonNetwork.LocalPlayer.ActorNumber - 1;
    }

    public void UpdateDealList()
    {

        if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasHome32 == true)
        {

                //generate a new list with update info
                for (int i = 0; i < GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].house3s2List.Count; i++)
                {

                    //skip empty 
                    if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].house3s2List.Count == 0)
                    {
                        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasHome32 = false;
                        continue;
                    }
                    if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].house3s2List[i].isSelected == true)
                    {
                        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].house3s2List.RemoveAt(i);
                    }

                }
            
        }
        if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasHome21 == true)
        {

                //generate a new list with update info
                for (int i = 0; i < GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].house2s1List.Count; i++)
                {

                    //skip empty 
                    if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].house2s1List.Count == 0)
                    {
                        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasHome21 = false;
                        continue;
                    }
                    if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].house2s1List[i].isSelected == true)
                    {
                        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].house2s1List.RemoveAt(i);
                    }
                }
            
        }
        if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasCondominium21 == true)
        {

                //generate a new list with update info
                for (int i = 0; i < GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].CondominiumList.Count; i++)
                {

                    //skip empty 
                    if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].CondominiumList.Count == 0)
                    {
                        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasCondominium21 = false;
                        continue;
                    }
                    if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].CondominiumList[i].isSelected == true)
                    {
                        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].CondominiumList.RemoveAt(i);
                    }

                }
            
        }
        if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hascommercialBuilding == true)
        {

                //generate a new list with update info
                for (int i = 0; i < GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].CommercialBuildingList.Count; i++)
                {

                    //skip empty 
                    if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].CommercialBuildingList.Count == 0)
                    {
                        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hascommercialBuilding = false;
                        continue;
                    }
                    if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].CommercialBuildingList[i].isSelected == true)
                    {
                        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].CommercialBuildingList.RemoveAt(i);
                    }
                }
            
        }
        if (MarketDeckController.instance.usedCards[MarketDeckController.instance.cardcount - 1].Apartment == true && GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasApartment == true)
        {

                //generate a new list with update info
                for (int i = 0; i < GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].ApartmentList.Count; i++)
                {

                    //skip empty 
                    if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].ApartmentList.Count == 0)
                    {
                        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasApartment = false;
                        continue;
                    }
                    if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].ApartmentList[i].isSelected == true)
                    {
                        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].ApartmentList.RemoveAt(i);
                    }
                }

            
        }

    }

    [PunRPC]
    public void UpdateDealIncomeInList()
    {


        if (MarketDeckController.instance.usedCards[MarketDeckController.instance.cardcount - 1].increaseIncome == true && GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasHome32 == true)
        {
            
                //generate a new list with update info
                for (int i = 0; i < GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].house3s2List.Count; i++)
                {

                    //skip empty 
                    if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].house3s2List.Count == 0)
                    {
                        continue;
                    }
                    if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].house3s2List[i].isSelected == false && GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].house3s2List[i].CashflowIncome <= 10000)
                    {
                        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].house3s2List[i].CashflowIncome += MarketDeckController.instance.usedCards[MarketDeckController.instance.cardcount - 1].Cost;
                    GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].income += MarketDeckController.instance.usedCards[MarketDeckController.instance.cardcount - 1].Cost;

                    }


                }
            
        }
        else if (MarketDeckController.instance.usedCards[MarketDeckController.instance.cardcount - 1].increaseIncome == true && GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasHome21 == true)
        {
            
                //generate a new list with update info
                for (int i = 0; i < GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].house2s1List.Count; i++)
                {

                    //skip empty 
                    if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].house2s1List.Count == 0)
                    {
                        continue;
                    }
                    if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].house2s1List[i].isSelected == false && GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].house2s1List[i].CashflowIncome <= 10000)
                    {
                        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].house2s1List[i].CashflowIncome += MarketDeckController.instance.usedCards[MarketDeckController.instance.cardcount - 1].Cost;
                    GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].income += MarketDeckController.instance.usedCards[MarketDeckController.instance.cardcount - 1].Cost;

                    }


                }
            
        }
        else if (MarketDeckController.instance.usedCards[MarketDeckController.instance.cardcount - 1].increaseIncome == true && GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasCondominium21 == true)
        {
            
                //generate a new list with update info
                for (int i = 0; i < GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].CondominiumList.Count; i++)
                {

                    //skip empty 
                    if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].CondominiumList.Count == 0)
                    {
                        continue;
                    }
                    if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].CondominiumList[i].isSelected == false && GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].CondominiumList[i].CashflowIncome <= 10000)
                    {
                        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].CondominiumList[i].CashflowIncome += MarketDeckController.instance.usedCards[MarketDeckController.instance.cardcount - 1].Cost;
                    GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].income += MarketDeckController.instance.usedCards[MarketDeckController.instance.cardcount - 1].Cost;

                    }

                }
            
        }
        else if (MarketDeckController.instance.usedCards[MarketDeckController.instance.cardcount - 1].increaseIncome == true && GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hascommercialBuilding == true)
        {
            
                //generate a new list with update info
                for (int i = 0; i < GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].CommercialBuildingList.Count; i++)
                {

                    //skip empty 
                    if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].CommercialBuildingList.Count == 0)
                    {
                        continue;
                    }
                    if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].CommercialBuildingList[i].isSelected == false && GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].CommercialBuildingList[i].CashflowIncome <= 10000)
                    {
                        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].CommercialBuildingList[i].CashflowIncome += MarketDeckController.instance.usedCards[MarketDeckController.instance.cardcount - 1].Cost;
                    GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].income += MarketDeckController.instance.usedCards[MarketDeckController.instance.cardcount - 1].Cost;

                    }

                }
            
        }
        else if (MarketDeckController.instance.usedCards[MarketDeckController.instance.cardcount - 1].increaseIncome == true && GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasApartment == true)
        {
            
                for (int i = 0; i < GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].ApartmentList.Count; i++)
                {
                    //skip empty 
                    if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].ApartmentList.Count == 0)
                    {
                        continue;
                    }
                    if (GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].ApartmentList[i].isSelected == false && GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].ApartmentList[i].CashflowIncome <= 10000)
                    {
                        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].ApartmentList[i].CashflowIncome += MarketDeckController.instance.usedCards[MarketDeckController.instance.cardcount - 1].Cost;
                    GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].income += MarketDeckController.instance.usedCards[MarketDeckController.instance.cardcount - 1].Cost;

                        
                    }

                }

            
        }
        
    }

    [PunRPC]
    void UpdateLessIncomeCount(int lessIncome,int x)
    {
        GameManager.instace.playerList[x].lessIncomeCount = lessIncome;

    }

    [PunRPC]
    void EndTurnPlayer()
    {
        UIController.instance.passButton.SetActive(IsMyTurn());
    }

    [PunRPC]
    void CreateMarketDeckStart(int selected)
    {
        activeCards.Add(tempDeck[selected]);
        tempDeck.RemoveAt(selected);
        //activeCards.AddRange(activeCards);
        //activeCards.Add(tempDeck[i]);
        //tempDeck.RemoveAt(i);
        //iterations++;

    }


    [PunRPC]
    void setUpdeckToEveryone()
    {

    SetUpDeck();
        
    }

    [PunRPC]
    void AddToUseCard()
    {
        usedCards.Add(activeCards[0]);
        cardcount++;
        activeCards.RemoveAt(0);


    }

    [PunRPC]
    void UpdateMoney(int money, int x)
    {
        GameManager.instace.playerList[x].money = money;
        GameManager.Note myNote = new GameManager.Note();
        myNote.CardName = "- " + usedCards[cardcount - 1].cardName;
        myNote.price = usedCards[cardcount - 1].Cost;
        GameManager.instace.playerList[GameManager.instace.activePlayer].Keep.Add(myNote);

        GameManager.Note myNote2 = new GameManager.Note();
        myNote2.CardName = "= ";
        myNote2.price = GameManager.instace.playerList[x].money;
        GameManager.instace.playerList[x].Keep.Add(myNote2);
    }

    [PunRPC]
    void UpdateDealListRPC()
    {
        UpdateDealList();
    }
    [PunRPC]
    void ShowCardToAllPlayerRPC()
    {
        UIController.instance.cardShow.enabled = true;
        UIController.instance.cardShow.sprite = activeCards[0].cardSprite;
    }

    [PunRPC]
    void CalculateMarketRPC(int money)
    {

        GameManager.instace.playerList[GameManager.instace.activePlayer].money = GameManager.instace.playerList[GameManager.instace.activePlayer].money - usedCards[cardcount - 1].Cost;
        GameManager.instace.playerList[GameManager.instace.activePlayer].money = money;
    }

    [PunRPC]
    void whoCanSellandCancelMarket()
    {
        if (usedCards[cardcount - 1].GoldCoins == true && GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasGoldCoins == true)
        {
            UIController.instance.MarketSellButton.SetActive(true);
        }
        else if (usedCards[cardcount - 1].house3s2 == true && GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasHome32 == true)
        {
            UIController.instance.MarketSellButton.SetActive(true);
        }
        else if (usedCards[cardcount - 1].house2s1 == true && GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasHome21 == true)
        {
            UIController.instance.MarketSellButton.SetActive(true);
        }
        else if (usedCards[cardcount - 1].Condominium == true && GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasCondominium21 == true)
        {
            UIController.instance.MarketSellButton.SetActive(true);
        }
        else if (usedCards[cardcount - 1].CommercialBuilding == true && GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hascommercialBuilding == true)
        {
            UIController.instance.MarketSellButton.SetActive(true);
        }
        else if (usedCards[cardcount - 1].Apartment == true && GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].hasApartment == true)
        {
            UIController.instance.MarketSellButton.SetActive(true);
        }

        for (int j = 0; j < GameManager.instace.playerList.Count; j++)
        {
            if (usedCards[cardcount - 1].GoldCoins == true && GameManager.instace.playerList[j].hasGoldCoins == true)
            {
                UIController.instance.cancelButton.SetActive(false);
            }
            else if (usedCards[cardcount - 1].house3s2 == true && GameManager.instace.playerList[j].hasHome32 == true)
            {
                UIController.instance.cancelButton.SetActive(false);
            }
            else if (usedCards[cardcount - 1].house2s1 == true && GameManager.instace.playerList[j].hasHome21 == true)
            {
                UIController.instance.cancelButton.SetActive(false);
            }
            else if (usedCards[cardcount - 1].Condominium == true && GameManager.instace.playerList[j].hasCondominium21 == true)
            {
                UIController.instance.cancelButton.SetActive(false);
            }
            else if (usedCards[cardcount - 1].CommercialBuilding == true && GameManager.instace.playerList[j].hascommercialBuilding == true)
            {
                UIController.instance.cancelButton.SetActive(false);
            }
            else if (usedCards[cardcount - 1].Apartment == true && GameManager.instace.playerList[j].hasApartment == true)
            {
                UIController.instance.cancelButton.SetActive(false);
            }
        }
    }

    [PunRPC]
    void valueUpdate()
    {
        UIController.instance.MyMoneyText.text = GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].money.ToString();
        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].allRecieve = GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].salary + GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].income;
        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].InstallmentsBank = GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].loanBank / 10;
        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].sumChild = GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].child * GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].perChild;
        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].paid = GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].tax + GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].homeMortgage + GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].learnMortgage + GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].carMortgage + GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].creditcardMortgage + GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].extraPay + GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].InstallmentsBank + GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].sumChild;
        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].getmoney = GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].allRecieve - GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].paid;
    }

}
