using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Threading;

public class ImageSample : MonoBehaviour {
  static private Image s_Image;
  
  void Start(){
    // TODO
    string url = "https://raw.githubusercontent.com/liveralmask/UnitySamples/master/Profiles/WebSampleProfile.png";
    
    s_Image = GameObject.Find( "Image" ).GetComponent< Image >();
    
    StartCoroutine( Get( url ) );
  }
  
  private IEnumerator Get( string url ){
    UnityWebRequest request = UnityWebRequest.Get( url );
    yield return request.SendWebRequest();
    if ( request.isNetworkError || request.isHttpError ){
      UnityEngine.Debug.LogError( request.error );
      EditorApplication.isPlaying = false;
    }else{
      byte[] data = request.downloadHandler.data;
      // TODO
#if true
      Thread thread = new Thread( OnThread );
      thread.Start( data );
#else
      OnThread( data );
#endif
    }
  }
  
  static private void OnThread( object arg ){
    SetImage( s_Image, (byte[])arg );
  }
  
  static private void SetImage( Image image, byte[] data ){
    image.sprite = CreateSprite( data );
    image.GetComponent< RectTransform >().sizeDelta = new Vector2( image.sprite.rect.width, image.sprite.rect.height );
  }
  
  static private Sprite CreateSprite( byte[] bytes ){
    // UnityException: SupportsTextureFormatNative can only be called from the main thread.
    // Constructors and field initializers will be executed from the loading thread when loading a scene.
    // Don't use this function in the constructor or field initializers, instead move initialization code to the Awake or Start function.
    Texture2D texture = new Texture2D( 0, 0 );
    ImageConversion.LoadImage( texture, bytes, false );
    return Sprite.Create( texture, new Rect( 0, 0, texture.width, texture.height ), Vector2.zero );
  }
}
