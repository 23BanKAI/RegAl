using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Concrete Unit classes
public class EasyUnit : Unit
{
    public EasyUnit()
    {
        Name = "Easy Unit";
        Health = 50;
    }
}

public class HardUnit : Unit
{
    public HardUnit()
    {
        Name = "Hard Unit";
        Health = 100;
    }
}

public class Enemy
{
    public string Type { get; set; }
    public int Damage { get; set; }

}


public class EasyEnemy : Enemy
{
    public EasyEnemy()
    {
        Type = "Easy Enemy";
        Damage = 5; 
    }
}

public class HardEnemy : Enemy
{
    public HardEnemy()
    {
        Type = "Hard Enemy";
        Damage = 20; 
    }
}

public class PowerUp
{
    public string PowerUpType { get; set; }
}

public class EasyPowerUp : PowerUp
{
    public EasyPowerUp()
    {
        PowerUpType = "Easy Boost";
    }
}

public class HardPowerUp : PowerUp
{
    public HardPowerUp()
    {
        PowerUpType = "Hard Boost";
    }
}
public interface IGameObjectFactory
{
    Unit CreateUnit();
    Enemy CreateEnemy();
    PowerUp CreatePowerUp();
}
public class EasyLevelFactory : IGameObjectFactory
{
    public Unit CreateUnit() { return new EasyUnit(); }
    public Enemy CreateEnemy() { return new EasyEnemy(); }
    public PowerUp CreatePowerUp() { return new EasyPowerUp(); }
}

public class HardLevelFactory : IGameObjectFactory
{
    public Unit CreateUnit() { return new HardUnit(); }
    public Enemy CreateEnemy() { return new HardEnemy(); }
    public PowerUp CreatePowerUp() { return new HardPowerUp(); }
}
public class GameManager
{
    private IGameObjectFactory factory;

    public GameManager(IGameObjectFactory factory)
    {
        this.factory = factory;
    }

    public void CreateGameObjects()
    {
        Unit unit = factory.CreateUnit();
        Enemy enemy = factory.CreateEnemy();
        PowerUp powerUp = factory.CreatePowerUp();
    }
}
public class gameManagerScript : MonoBehaviour
{
    private static gameManagerScript _instance;

    public static gameManagerScript Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<gameManagerScript>();
                if (_instance == null)
                {
                    GameObject container = new GameObject("gameManagerScript");
                    _instance = container.AddComponent<gameManagerScript>();
                }
            }
            return _instance;
        }
    }

    [Header("UI GameObjects")]
    public TMP_Text currentTeamUI;
    public Canvas displayWinnerUI;

    public TMP_Text UIunitCurrentHealth;
    public TMP_Text UIunitAttackDamage;
    public TMP_Text UIunitAttackRange;
    public TMP_Text UIunitMoveSpeed;
    public TMP_Text UIunitName;
    public UnityEngine.UI.Image UIunitSprite;

    public Canvas UIunitCanvas;
    public GameObject playerPhaseBlock;
    private Animator playerPhaseAnim;
    private TMP_Text playerPhaseText;
   
    //Raycast for the update for unitHover info
    private Ray ray;
    private RaycastHit hit;

   
    public int numberOfTeams = 2;
    public int currentTeam;
    public GameObject unitsOnBoard;

    public GameObject team1;
    public GameObject team2;

    public GameObject unitBeingDisplayed;
    public GameObject tileBeingDisplayed;
    public bool displayingUnitInfo;

    public tileMapScript TMS;

    //Cursor Info for tileMapScript
    public int cursorX;
    public int cursorY;
    //currentTileBeingMousedOver
    public int selectedXTile;
    public int selectedYTile;

    //Variables for unitPotentialMovementRoute
    List<Node> currentPathForUnitRoute;
    List<Node> unitPathToCursor;

    public bool unitPathExists;

    public Material UIunitRoute;
    public Material UIunitRouteCurve;
    public Material UIunitRouteArrow;
    public Material UICursor;

    public int routeToX;
    public int routeToY;

    public GameObject quadThatIsOneAwayFromUnit;

   
    public void Start()
    {
        currentTeam = 0;
        setCurrentTeamUI();
        teamHealthbarColorUpdate();
        displayingUnitInfo = false;
        playerPhaseAnim = playerPhaseBlock.GetComponent<Animator>();
        playerPhaseText = playerPhaseBlock.GetComponentInChildren<TextMeshProUGUI>();
        unitPathToCursor = new List<Node>();
        unitPathExists = false;       
      
        TMS = GetComponent<tileMapScript>();

        
    }

    public void Update()
    {

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            //Update cursorLocation and unit appearing in the topLeft
            cursorUIUpdate();
            unitUIUpdate();
            if (TMS.selectedUnit != null && TMS.selectedUnit.GetComponent<UnitScript>().getMovementStateEnum(1) == TMS.selectedUnit.GetComponent<UnitScript>().unitMoveState)
            {
                if (TMS.selectedUnitMoveRange.Contains(TMS.graph[cursorX, cursorY]))
                {
                    

                    
                    if (cursorX != TMS.selectedUnit.GetComponent<UnitScript>().x || cursorY != TMS.selectedUnit.GetComponent<UnitScript>().y)
                    {
                        if (!unitPathExists&&TMS.selectedUnit.GetComponent<UnitScript>().movementQueue.Count==0)
                        {
                           
                            unitPathToCursor = generateCursorRouteTo(cursorX, cursorY);
                           
                            routeToX = cursorX;
                            routeToY = cursorY;

                            if (unitPathToCursor.Count != 0)
                            {
                                                                                              
                                for(int i = 0; i < unitPathToCursor.Count; i++)
                                {
                                    int nodeX = unitPathToCursor[i].x;
                                    int nodeY = unitPathToCursor[i].y;

                                    if (i == 0)
                                    {
                                        GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
                                        quadToUpdate.GetComponent<Renderer>().material = UICursor;
                                    }
                                    else if (i!=0 && (i+1)!=unitPathToCursor.Count)
                                    {
                                        setCorrectRouteWithInputAndOutput(nodeX, nodeY,i);
                                    }
                                    else if (i == unitPathToCursor.Count-1)
                                    {
                                        setCorrectRouteFinalTile(nodeX, nodeY, i);
                                    }
                                    
                                    TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY].GetComponent<Renderer>().enabled = true;
                                   
                                }
                                    
                            }
                            unitPathExists = true;
                          
                        }
                        
                        else if (routeToX != cursorX || routeToY != cursorY)
                        {
                           
                            if (unitPathToCursor.Count != 0)
                            {
                                for (int i = 0; i < unitPathToCursor.Count; i++)
                                {
                                    int nodeX = unitPathToCursor[i].x;
                                    int nodeY = unitPathToCursor[i].y;

                                    TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY].GetComponent<Renderer>().enabled = false;
                                }
                            }
                            
                            unitPathExists = false;
                        }
                    }
                    else if(cursorX == TMS.selectedUnit.GetComponent<UnitScript>().x && cursorY == TMS.selectedUnit.GetComponent<UnitScript>().y)
                    {
                        
                        TMS.disableUnitUIRoute();
                        unitPathExists = false;
                    }
                    
                }               
            }
        
        }
        
    }

    public void setCurrentTeamUI()
    {
        currentTeamUI.SetText("Текущий игрок : Игрок " + (currentTeam+1).ToString());
    }
    public void switchCurrentPlayer()
    {
        resetUnitsMovements(returnTeam(currentTeam));
        currentTeam++;
        if (currentTeam == numberOfTeams)
        {
            currentTeam = 0;
        }
        
    }

    public GameObject returnTeam(int i)
    {
        GameObject teamToReturn = null;
        if (i == 0)
        {
            teamToReturn = team1;
        }
        else if (i == 1)
        {
            teamToReturn = team2;
        }
        return teamToReturn;
    }

    public void resetUnitsMovements(GameObject teamToReset)
    {
        foreach (Transform unit in teamToReset.transform)
        {
            unit.GetComponent<UnitScript>().moveAgain();
        }
    }


    public void endTurn()
    {
        
        if (TMS.selectedUnit == null)
        {
            switchCurrentPlayer();
            if (currentTeam == 1)
            {
                playerPhaseAnim.SetTrigger("slideLeftTrigger");
                playerPhaseText.SetText("Ход игрока №1");
            }
            else if (currentTeam == 0)
            {
                playerPhaseAnim.SetTrigger("slideRightTrigger");
                playerPhaseText.SetText("Ход игрока №2");
            }
            teamHealthbarColorUpdate();
            setCurrentTeamUI();
        }
    }


    public void checkIfUnitsRemain(GameObject unit, GameObject enemy)
    {
        //  Debug.Log(team1.transform.childCount);
        //  Debug.Log(team2.transform.childCount);
        StartCoroutine(checkIfUnitsRemainCoroutine(unit,enemy));
    }


    public void cursorUIUpdate()
    {
       //If we are mousing over a tile, highlight it
        if (hit.transform.CompareTag("Tile"))
        {
            if (tileBeingDisplayed == null)
            {
                selectedXTile = hit.transform.gameObject.GetComponent<ClickableTileScript>().tileX;
                selectedYTile = hit.transform.gameObject.GetComponent<ClickableTileScript>().tileY;
                cursorX = selectedXTile;
                cursorY = selectedYTile;
                TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingDisplayed = hit.transform.gameObject;
                
            }
            else if (tileBeingDisplayed != hit.transform.gameObject)
            {
                selectedXTile = tileBeingDisplayed.GetComponent<ClickableTileScript>().tileX;
                selectedYTile = tileBeingDisplayed.GetComponent<ClickableTileScript>().tileY;
                TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;

                selectedXTile = hit.transform.gameObject.GetComponent<ClickableTileScript>().tileX;
                selectedYTile = hit.transform.gameObject.GetComponent<ClickableTileScript>().tileY;
                cursorX = selectedXTile;
                cursorY = selectedYTile;
                TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingDisplayed = hit.transform.gameObject;
                
            }

        }
        else if (hit.transform.CompareTag("Unit"))
        {
            if (tileBeingDisplayed == null)
            {
                selectedXTile = hit.transform.parent.gameObject.GetComponent<UnitScript>().x;
                selectedYTile = hit.transform.parent.gameObject.GetComponent<UnitScript>().y;
                cursorX = selectedXTile;
                cursorY = selectedYTile;
                TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingDisplayed = hit.transform.parent.gameObject.GetComponent<UnitScript>().tileBeingOccupied;

            }
            else if (tileBeingDisplayed != hit.transform.gameObject)
            {
                if (hit.transform.parent.gameObject.GetComponent<UnitScript>().movementQueue.Count == 0)
                {
                    selectedXTile = tileBeingDisplayed.GetComponent<ClickableTileScript>().tileX;
                    selectedYTile = tileBeingDisplayed.GetComponent<ClickableTileScript>().tileY;
                    TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;

                    selectedXTile = hit.transform.parent.gameObject.GetComponent<UnitScript>().x;
                    selectedYTile = hit.transform.parent.gameObject.GetComponent<UnitScript>().y;
                    cursorX = selectedXTile;
                    cursorY = selectedYTile;
                    TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                    tileBeingDisplayed = hit.transform.parent.GetComponent<UnitScript>().tileBeingOccupied;
                   
                }
               
            }
        }

        else
        {
            TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;
        }
    }



    public void unitUIUpdate()
    {
        if (!displayingUnitInfo)
        {
            if (hit.transform.CompareTag("Unit"))
            {
                UIunitCanvas.enabled = true;
                displayingUnitInfo = true;
                unitBeingDisplayed = hit.transform.parent.gameObject;
                var highlightedUnitScript = hit.transform.parent.gameObject.GetComponent<UnitScript>();

                UIunitCurrentHealth.SetText(highlightedUnitScript.currentHealthPoints.ToString());
                UIunitAttackDamage.SetText(highlightedUnitScript.attackDamage.ToString());
                UIunitAttackRange.SetText(highlightedUnitScript.attackRange.ToString());
                UIunitMoveSpeed.SetText(highlightedUnitScript.moveSpeed.ToString());
                UIunitName.SetText(highlightedUnitScript.unitName);
                UIunitSprite.sprite = highlightedUnitScript.unitSprite;
                
            }
            else if (hit.transform.CompareTag("Tile"))
            {
                if (hit.transform.GetComponent<ClickableTileScript>().unitOnTile != null)
                {
                    unitBeingDisplayed = hit.transform.GetComponent<ClickableTileScript>().unitOnTile;

                    UIunitCanvas.enabled = true;
                    displayingUnitInfo = true;
                    var highlightedUnitScript = unitBeingDisplayed.GetComponent<UnitScript>();

                    UIunitCurrentHealth.SetText(highlightedUnitScript.currentHealthPoints.ToString());
                    UIunitAttackDamage.SetText(highlightedUnitScript.attackDamage.ToString());
                    UIunitAttackRange.SetText(highlightedUnitScript.attackRange.ToString());
                    UIunitMoveSpeed.SetText(highlightedUnitScript.moveSpeed.ToString());
                    UIunitName.SetText(highlightedUnitScript.unitName);
                    UIunitSprite.sprite = highlightedUnitScript.unitSprite;

                }
            }
        }
        else if (hit.transform.gameObject.CompareTag("Tile"))
        {
            if (hit.transform.GetComponent<ClickableTileScript>().unitOnTile == null)
            {
                UIunitCanvas.enabled = false;
                displayingUnitInfo = false;
            }
            else if (hit.transform.GetComponent<ClickableTileScript>().unitOnTile != unitBeingDisplayed)
            {
                UIunitCanvas.enabled = false;
                displayingUnitInfo = false;
            }
        }
        else if (hit.transform.gameObject.CompareTag("Unit"))
        {
            if (hit.transform.parent.gameObject != unitBeingDisplayed)
            {
                UIunitCanvas.enabled = false;
                displayingUnitInfo = false;
            }
        }
    }


    public void teamHealthbarColorUpdate()
    {
        for(int i = 0; i < numberOfTeams; i++)
        {
            GameObject team = returnTeam(i);
            if(team == returnTeam(currentTeam))
            {
                foreach (Transform unit in team.transform)
                {
                    unit.GetComponent<UnitScript>().changeHealthBarColour(0);
                }
            }
            else
            {
                foreach (Transform unit in team.transform)
                {
                    unit.GetComponent<UnitScript>().changeHealthBarColour(1);
                }
            }
        }
       
        
    }

    public List<Node> generateCursorRouteTo(int x, int y)
    {

        if (TMS.selectedUnit.GetComponent<UnitScript>().x == x && TMS.selectedUnit.GetComponent<UnitScript>().y == y)
        {
            Debug.Log("clicked the same tile that the unit is standing on");
            currentPathForUnitRoute = new List<Node>();
            

            return currentPathForUnitRoute;
        }
        if (TMS.unitCanEnterTile(x, y) == false)
        {

            return null;
        }

        currentPathForUnitRoute = null;
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        Node source = TMS.graph[TMS.selectedUnit.GetComponent<UnitScript>().x, TMS.selectedUnit.GetComponent<UnitScript>().y];
        Node target = TMS.graph[x, y];
        dist[source] = 0;
        prev[source] = null;
        List<Node> unvisited = new List<Node>();

        //Initialize
        foreach (Node n in TMS.graph)
        {

            if (n != source)
            {
                dist[n] = Mathf.Infinity;
                prev[n] = null;
            }
            unvisited.Add(n);
        }
        while (unvisited.Count > 0)
        {
            Node u = null;
            foreach (Node possibleU in unvisited)
            {
                if (u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }


            if (u == target)
            {
                break;
            }

            unvisited.Remove(u);

            foreach (Node n in u.neighbours)
            {

                float alt = dist[u] + TMS.costToEnterTile(n.x, n.y);
                if (alt < dist[n])
                {
                    dist[n] = alt;
                    prev[n] = u;
                }
            }
        }
        if (prev[target] == null)
        {
            //No route;
            return null;
        }
        currentPathForUnitRoute = new List<Node>();
        Node curr = target;
        while (curr != null)
        {
            currentPathForUnitRoute.Add(curr);
            curr = prev[curr];
        }
        currentPathForUnitRoute.Reverse();

        return currentPathForUnitRoute;




    }

    public void resetQuad(GameObject quadToReset)
    {
        quadToReset.GetComponent<Renderer>().material = UICursor;
        quadToReset.transform.eulerAngles = new Vector3(90, 0, 0);
        
    }

    public void UIunitRouteArrowDisplay(Vector2 cursorPos,Vector3 arrowRotationVector)
    {
        GameObject quadToManipulate = TMS.quadOnMapForUnitMovementDisplay[(int)cursorPos.x, (int)cursorPos.y];
        quadToManipulate.transform.eulerAngles = arrowRotationVector;
        quadToManipulate.GetComponent<Renderer>().material = UIunitRouteArrow;
        quadToManipulate.GetComponent<Renderer>().enabled = true;
    }

    public Vector2 directionBetween(Vector2 currentVector, Vector2 nextVector)
    {

        
        Vector2 vectorDirection = (nextVector - currentVector).normalized;
       
        if (vectorDirection == Vector2.right)
        {
            return Vector2.right;
        }
        else if (vectorDirection == Vector2.left)
        {
            return Vector2.left;
        }
        else if (vectorDirection == Vector2.up)
        {
            return Vector2.up;
        }
        else if (vectorDirection == Vector2.down)
        {
            return Vector2.down;
        }
        else
        {
            Vector2 vectorToReturn = new Vector2();
            return vectorToReturn;
        }
    }

    public void setCorrectRouteWithInputAndOutput(int nodeX,int nodeY,int i)
    {
        Vector2 previousTile = new Vector2(unitPathToCursor[i - 1].x + 1, unitPathToCursor[i - 1].y + 1);
        Vector2 currentTile = new Vector2(unitPathToCursor[i].x + 1, unitPathToCursor[i].y + 1);
        Vector2 nextTile = new Vector2(unitPathToCursor[i + 1].x + 1, unitPathToCursor[i + 1].y + 1);

        Vector2 backToCurrentVector = directionBetween(previousTile, currentTile);
        Vector2 currentToFrontVector = directionBetween(currentTile, nextTile);


        //Right (UP/DOWN/RIGHT)
        if (backToCurrentVector == Vector2.right && currentToFrontVector == Vector2.right)
        {
            //Debug.Log("[IN[R]]->[Out[R]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRoute;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.right && currentToFrontVector == Vector2.up)
        {
            //Debug.Log("[IN[R]]->[Out[UP]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 180);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;

        }
        else if (backToCurrentVector == Vector2.right && currentToFrontVector == Vector2.down)
        {
            //Debug.Log("[IN[R]]->[Out[DOWN]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        //Left (UP/DOWN/LEFT)
        else if (backToCurrentVector == Vector2.left && currentToFrontVector == Vector2.left)
        {
            //Debug.Log("[IN[L]]->[Out[L]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRoute;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.left && currentToFrontVector == Vector2.up)
        {
            //Debug.Log("[IN[L]]->[Out[UP]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.left && currentToFrontVector == Vector2.down)
        {
            //Debug.Log("[IN[L]]->[Out[DOWN]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        //UP (UP/RIGHT/LEFT)
        else if (backToCurrentVector == Vector2.up && currentToFrontVector == Vector2.up)
        {
            //Debug.Log("[IN[UP]]->[Out[UP]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRoute;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.up && currentToFrontVector == Vector2.right)
        {
            //Debug.Log("[IN[UP]]->[Out[R]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.up && currentToFrontVector == Vector2.left)
        {
            //Debug.Log("[IN[UP]]->[Out[L]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        //DOWN (DOWN/RIGHT/LEFT)
        else if (backToCurrentVector == Vector2.down && currentToFrontVector == Vector2.down)
        {
            //Debug.Log("[IN[DOWN]]->[Out[DOWN]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRoute;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.down && currentToFrontVector == Vector2.right)
        {
            //Debug.Log("[IN[DOWN]]->[Out[R]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;

        }
        else if (backToCurrentVector == Vector2.down && currentToFrontVector == Vector2.left)
        {
            //Debug.Log("[IN[DOWN]]->[Out[L]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 180);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
    }

    public void setCorrectRouteFinalTile(int nodeX,int nodeY,int i)
    {
        Vector2 previousTile = new Vector2(unitPathToCursor[i - 1].x + 1, unitPathToCursor[i - 1].y + 1);
        Vector2 currentTile = new Vector2(unitPathToCursor[i].x + 1, unitPathToCursor[i].y + 1);
        Vector2 backToCurrentVector = directionBetween(previousTile, currentTile);

        if (backToCurrentVector == Vector2.right)
        {
            //Debug.Log("[IN[R]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteArrow;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.left)
        {
            //Debug.Log("[IN[L]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteArrow;
            quadToUpdate.GetComponent<Renderer>().enabled = true;

        }
        else if (backToCurrentVector == Vector2.up)
        {
            //Debug.Log("[IN[U]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteArrow;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.down)
        {
            //Debug.Log("[IN[D]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 180);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteArrow;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
    }


    public IEnumerator checkIfUnitsRemainCoroutine(GameObject unit, GameObject enemy)
    {
        while (unit.GetComponent<UnitScript>().combatQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }
        
        while (enemy.GetComponent<UnitScript>().combatQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }
        if (team1.transform.childCount == 0)
        {
            displayWinnerUI.enabled = true;
            displayWinnerUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Игрок №2 выиграл!");
           
            
        }
        else if (team2.transform.childCount == 0)
        {
            displayWinnerUI.enabled = true;
            displayWinnerUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Игрок №1 выиграл!");

          
        }


    }

    
    public void win()
    {
        displayWinnerUI.enabled = true;
        displayWinnerUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Победитель!");

    }

  
   
}
