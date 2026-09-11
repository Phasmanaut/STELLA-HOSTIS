using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Bullet : MonoBehaviour
{
    GameObject bullet;
    float speed =10;
   
    void Update()
    {
        transform.Translate(0,speed * Time.deltaTime , 0);


        //keeps the bullet in the playing field
        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
    }


 void OnTriggerEnter(Collider col)  //killed on hit
    {
        if (col.gameObject.tag == "Destroy" || col.gameObject.tag == "Enemy")
        {
            Destroy (this.gameObject);
        }
    }




}
