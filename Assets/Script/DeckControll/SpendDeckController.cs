using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;


public class SpendDeckController : MonoBehaviourPunCallbacks
{
    public static SpendDeckController instance;
    public int cardcount=0; 

    private void Awake()
    {
        instance = this;
        //PhotonPeer.RegisterType(typeof(CardSpendScriptableObj), 0, CardSpendScriptableObjSerialization.Serialize, CardSpendScriptableObjSerialization.Deserialize);

    }

    public List<CardSpendScriptableObj> deckToUse = new List<CardSpendScriptableObj>();

    public List<CardSpendScriptableObj> activeCards = new List<CardSpendScriptableObj>();

    public List<CardSpendScriptableObj> usedCards = new List<CardSpendScriptableObj>();

    public List<CardSpendScriptableObj> tempDeck = new List<CardSpendScriptableObj>();

    //int iterations = 0;

    public SpendCard cardsToSpawns;

    public int FordrawCard = 1;
    public float waitBetweenDrawingCard = .3f;


    // Start is called before the first frame update
    void Start()
    {
        //PhotonPeer.RegisterType(typeof(CardSpendScriptableObj), 0, CardSpendScriptableObjSerialization.Serialize, CardSpendScriptableObjSerialization.Deserialize);
        //photonView.RPC("setUpdeckToEveryone", RpcTarget.All);
        SetUpDeck();
        UIController.instance.drawButton.SetActive(false);
        UIController.instance.cardShow.enabled = false;
        UIController.instance.cancelButton.SetActive(false);
        UIController.instance.passButton.SetActive(false);
        UIController.instance.payButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
  
    }

    public void SetUpDeck()
    {
        
        

        //photonView.RPC("CreateSpendDeckStart", RpcTarget.All);
        /*
        activeCards.Clear();
        List<CardSpendScriptableObj> tempDeck = new List<CardSpendScriptableObj>();
        tempDeck.AddRange(deckToUse);

        int iterations = 0;
        while (tempDeck.Count > 0 && iterations < 500)
        {
            int selected = Random.Range(0, tempDeck.Count);

            activeCards.Add(tempDeck[selected]);
            tempDeck.RemoveAt(selected);

            iterations++;
        }*/
        activeCards.Clear();
        tempDeck.AddRange(deckToUse);
        
        if (PhotonNetwork.IsMasterClient)
        {
            //activeCards.Clear();
            //List<CardSpendScriptableObj> tempDeck = new List<CardSpendScriptableObj>();
            //tempDeck.AddRange(deckToUse);

            int iterations = 0;
            while (tempDeck.Count > 0 && iterations < 500)
            {
                int selected = Random.Range(0, tempDeck.Count);
                photonView.RPC("CreateSpendDeckStart", RpcTarget.All,selected);
                //activeCards.Add(tempDeck[selected]);
                //tempDeck.RemoveAt(selected);

                iterations++;
            }
            //photonView.RPC("CreateSpendDeckStart", RpcTarget.All);
        }

    }

    public void DrawCardToHand()
    {


        SpendCard newCard = Instantiate(cardsToSpawns, transform.position, transform.rotation);
        newCard.cardSpendSO = activeCards[0];


        //UIController.instance.cardShow.enabled = true;

        //UIController.instance.loanButton.SetActive(true);
        UIController.instance.payButton.SetActive(true);
        UIController.instance.drawButton.SetActive(false);

        //ShowController.instance.AddCardToShow(newCard);
        photonView.RPC("ShowCardToAllPlayerRPC", RpcTarget.All);
        //GameManager.instace.playerList[GameManager.instace.activePlayer].money = GameManager.instace.playerList[GameManager.instace.activePlayer].money - activeCards[0].payCost;

        //UIController.instance.cardShow.sprite = activeCards[0].cardSprite;

        photonView.RPC("AddToUseCard", RpcTarget.All);
        /*
        usedCards.Add(activeCards[0]);
        cardcount++;
        activeCards.RemoveAt(0);
        
        Destroy(newCard.gameObject, 1);*/
        //GameManager.instace.state = GameManager.States.SWITCH_PLAYER;
        Destroy(newCard.gameObject, 1);
    }

    public void PayCost()
    {

        if(usedCards[cardcount - 1].hasChildsOrNot == true && GameManager.instace.playerList[GameManager.instace.activePlayer].hasChild == false)
        {
            UIController.instance.drawButton.SetActive(false);
            UIController.instance.cardShow.enabled = false;
            UIController.instance.payButton.SetActive(false);
            GameManager.instace.playerList[GameManager.instace.activePlayer].isSpendAlready = true;
            UIController.instance.passButton.SetActive(true);
        }
        else
        {
            if (GameManager.instace.playerList[GameManager.instace.activePlayer].money >= usedCards[cardcount - 1].payCost)
            {
                GameManager.instace.playerList[GameManager.instace.activePlayer].money = GameManager.instace.playerList[GameManager.instace.activePlayer].money - usedCards[cardcount - 1].payCost;
                photonView.RPC("UpdateMoney", RpcTarget.All, GameManager.instace.playerList[GameManager.instace.activePlayer].money, GameManager.instace.activePlayer);
                UIController.instance.drawButton.SetActive(false);
                UIController.instance.cardShow.enabled = false;
                UIController.instance.payButton.SetActive(false);
                GameManager.instace.playerList[GameManager.instace.activePlayer].isSpendAlready = true;
                UIController.instance.passButton.SetActive(true);
            }
            else if (GameManager.instace.playerList[GameManager.instace.activePlayer].getmoney <= 0 && GameManager.instace.playerList[GameManager.instace.activePlayer].money < usedCards[cardcount - 1].payCost)
            {
                //lose
                GameManager.instace.playerList[GameManager.instace.activePlayer].playerType = GameManager.Entity.PlayerTypes.NO_PLAYER;

                GameManager.instace.playerInRoom = 0;
                for (int i = 0; i < GameManager.instace.playerList.Count; i++)
                {
                    if(GameManager.instace.playerList[i].playerType == GameManager.Entity.PlayerTypes.NO_PLAYER)
                    {
                        GameManager.instace.playerInRoom++;
                    }
                    
                }
                if(GameManager.instace.playerInRoom > 0)
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

        }
        

    }

    public void Loan()
    {

    }
    


    private bool IsMyTurn()
    {
        // Replace with your logic. This could be checking against a player list, an ID, etc.
        return GameManager.instace.activePlayer == PhotonNetwork.LocalPlayer.ActorNumber - 1;
    }

    [PunRPC]
    void setUpdeckToEveryone()
    {
        SetUpDeck();

    }
    [PunRPC]
    void EndTurnPlayer()
    {
        UIController.instance.passButton.SetActive(IsMyTurn());
    }

    [PunRPC]
    void CreateSpendDeckStart(int selected)
    {
        activeCards.Add(tempDeck[selected]);
        tempDeck.RemoveAt(selected);
        //activeCards.AddRange(activeCards);
        //activeCards.Add(tempDeck[i]);
        //tempDeck.RemoveAt(i);
        //iterations++;

    }

    [PunRPC]
    void AddToUseCard()
    {
        usedCards.Add(activeCards[0]);
        cardcount++;
        activeCards.RemoveAt(0);

        
    }

    [PunRPC]
    void UpdateMoney(int money,int x)
    {
        GameManager.instace.playerList[x].money = money;
        UIController.instance.MyMoneyText.text = GameManager.instace.playerList[x].money.ToString();
        GameManager.Note myNote = new GameManager.Note();
        myNote.CardName = "- " + usedCards[cardcount - 1].cardName;
        myNote.price = usedCards[cardcount - 1].payCost;
        GameManager.instace.playerList[x].Keep.Add(myNote);

        GameManager.Note myNote2 = new GameManager.Note();
        myNote2.CardName = "= ";
        myNote2.price = GameManager.instace.playerList[x].money;
        GameManager.instace.playerList[x].Keep.Add(myNote2);
    }

    [PunRPC]
    void ShowCardToAllPlayerRPC()
    {
        UIController.instance.cardShow.enabled = true;
        UIController.instance.cardShow.sprite = activeCards[0].cardSprite;
    }

    [PunRPC]
    void CalculateSpendRPC(int money)
    {
        

        GameManager.instace.playerList[GameManager.instace.activePlayer].money = GameManager.instace.playerList[GameManager.instace.activePlayer].money - usedCards[cardcount - 1].payCost ;
        GameManager.instace.playerList[GameManager.instace.activePlayer].money = money;
    }

}