using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class WebSample : MonoBehaviour {
  void Start(){
    // TODO
    string url = "https://www.google.com";
    
    StartCoroutine( Get( url ) );
  }
  
  private IEnumerator Get( string url ){
    yield return new WaitForSeconds( 1f );
    
    UnityWebRequest request = UnityWebRequest.Get( url );
    yield return request.SendWebRequest();
    if ( request.isNetworkError || request.isHttpError ){
      EditorApplication.isPaused = true;
    }else{
      EditorApplication.isPlaying = false;
    }
  }
}
