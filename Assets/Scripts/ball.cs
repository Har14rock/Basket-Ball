using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.UI;

public class ball : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioClip audioBola, audioCanasta;
    AudioSource fuenteDeAudio;
    

    public float force = 100f;

    public GameObject ballPrediction;

    private Vector2 startPosition;

    private Vector2 defaultBallPosition;

    public int maxTrajectoryInteraction = 50;

    public UnityEvent scoredEvent;

    private Rigidbody2D physics;

    private Scene sceneMain;
    private PhysicsScene2D sceneMainPhysics;

    private Scene scenePrediction;
    private PhysicsScene2D scenePredictionPhysics;

    private float ballScorePosition;

    void Awake() {
        physics = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        Physics2D.simulationMode = SimulationMode2D.Script;
        physics.isKinematic = true;
        defaultBallPosition = transform.position;

        createSceneMain();
        createScenePrediction();
        fuenteDeAudio = GetComponent<AudioSource>();
    }



    // Update is called once per frame
    void Update()
    {
        
        if(Input.GetMouseButtonDown(0)){
            
            startPosition = getMousePosition();
           
        }

        if(Input.GetMouseButton(0)){

            GameObject newBallPrediction = spawnBallPrediction();
            throwBall(newBallPrediction.GetComponent<Rigidbody2D>());
            
            createTrajectory(newBallPrediction);
            Destroy(newBallPrediction);

        }

        if(Input.GetMouseButtonUp(0)){
            
            GetComponent<LineRenderer>().positionCount = 0;
            
            physics.isKinematic = false;
            throwBall(physics);
        }

    }

    void FixedUpdate() {
        if(!sceneMainPhysics.IsValid()) return;
      

        sceneMainPhysics.Simulate(Time.fixedDeltaTime);
        
    }


    /*-------------------------------METHODS----------------------------------------*/

    private void createTrajectory(GameObject newBallPrediction){

     LineRenderer ballLine = GetComponent<LineRenderer>();
        ballLine.positionCount = maxTrajectoryInteraction;
            for(int i=0; i<maxTrajectoryInteraction; i++){
                scenePredictionPhysics.Simulate(Time.fixedDeltaTime);
                ballLine.SetPosition(i, new Vector3(newBallPrediction.transform.position.x, newBallPrediction.transform.position.y,0));
            }

    }

    private void throwBall(Rigidbody2D physics){
        physics.AddForce(getThrowPower(startPosition,getMousePosition()), ForceMode2D.Force);
    }

    private GameObject spawnBallPrediction(){

        GameObject newBallPrediction = GameObject.Instantiate(ballPrediction);
        SceneManager.MoveGameObjectToScene(newBallPrediction, scenePrediction);
        newBallPrediction.transform.position = transform.position;
        return newBallPrediction;

    }

    private Vector2 getThrowPower(Vector2 startPosition, Vector2 endPosition){
        return (startPosition - endPosition) * force;
    }


    void OnCollisionEnter2D(Collision2D collision) {
        if(!collision.gameObject.tag.Equals("ground")) return;
        fuenteDeAudio.PlayOneShot(audioBola);
        physics.isKinematic = true;
        transform.position = defaultBallPosition;
        physics.velocity = Vector2.zero;
        physics.angularVelocity = 0f;
        
       
    }

    void OnTriggerEnter2D(Collider2D collider) {
        ballScorePosition = transform.position.y;
    }

    void OnTriggerExit2D(Collider2D collider) {
        if(transform.position.y < ballScorePosition){
            Debug.Log("Scored");
            fuenteDeAudio.PlayOneShot(audioCanasta);
            scoredEvent.Invoke();
           
            
        }
    }

    private Vector2 getMousePosition(){
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }



    private void createSceneMain(){

        sceneMain = SceneManager.CreateScene("MainScene");
        sceneMainPhysics = sceneMain.GetPhysicsScene2D();

    }

    private void createScenePrediction(){

        CreateSceneParameters sceneParameters = new CreateSceneParameters(LocalPhysicsMode.Physics2D);

        scenePrediction = SceneManager.CreateScene("PredictionScene", sceneParameters);
        scenePredictionPhysics = scenePrediction.GetPhysicsScene2D();

    }

}
