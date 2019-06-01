using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class AudioSample : MonoBehaviour {
  static private int s_Frequency = 44100;
  
  static private Queue< float[] > s_AudioDataQueue;
  static private int s_AudioDataQueueMaxCount = 100;
  
  static private string s_MicrophoneDeviceName;
  static private int s_MicrophonePosition;
  static private int s_MicrophoneMaxCount;
  static private AudioClip s_MicrophoneAudioClip;
  
  static private AudioSource s_SpeakerAudioSource;
  static private AudioClip s_SpeakerAudioClip;
  static private int s_SpeakerLastPosition;
  static private int s_SpeakerStopPosition;
  static private int s_SpeakerMaxCount;
  
  private enum Mode {
    RecordingMicrophone = 1,
    PlayingSpeaker      = 2,
  };
  static private Mode s_Mode = Mode.RecordingMicrophone;
  
  static private Thread s_Thread;
  
  static private void OnThread(){
    while ( null != s_Thread ){
      UpdateAudio();
    }
  }
  
  void Start(){
    s_AudioDataQueue = new Queue< float[] >( s_AudioDataQueueMaxCount );
    
    s_MicrophoneDeviceName = Microphone.devices[ 0 ];
    s_MicrophoneAudioClip = Microphone.Start( s_MicrophoneDeviceName, true, 1, s_Frequency );
    s_MicrophonePosition = Microphone.GetPosition( s_MicrophoneDeviceName );
    s_MicrophoneMaxCount = GetMaxCount( s_MicrophoneAudioClip );
    
    s_SpeakerAudioSource = gameObject.GetComponent< AudioSource >();
    if ( null == s_SpeakerAudioSource ) s_SpeakerAudioSource = gameObject.AddComponent< AudioSource >();
    s_SpeakerAudioClip = AudioClip.Create( "Speaker", s_Frequency, 1, s_Frequency, false );
    s_SpeakerAudioSource.loop = true;
    s_SpeakerAudioSource.clip = s_SpeakerAudioClip;
    s_SpeakerMaxCount = GetMaxCount( s_SpeakerAudioClip );
    UnityEngine.Debug.Log( s_Mode +" Start" );
    
    // TODO
#if true
    s_Thread = new Thread( OnThread );
    s_Thread.Start();
#endif
  }
  
  void Update(){
    if ( null == s_Thread ) UpdateAudio();
  }
  
  static private void UpdateAudio(){
    switch ( s_Mode ){
    case Mode.RecordingMicrophone:{
      int position = Microphone.GetPosition( s_MicrophoneDeviceName );
      if ( s_MicrophonePosition == position ) return;
      
      int count = GetCount( s_MicrophonePosition, position, s_MicrophoneMaxCount );
      float[] data = new float[ count ];
      // UnityException: GetData can only be called from the main thread.
      // Constructors and field initializers will be executed from the loading thread when loading a scene.
      // Don't use this function in the constructor or field initializers, instead move initialization code to the Awake or Start function.
      s_MicrophoneAudioClip.GetData( data, s_MicrophonePosition );
      s_MicrophonePosition = position;
      s_AudioDataQueue.Enqueue( data );
      if ( s_AudioDataQueueMaxCount <= s_AudioDataQueue.Count ){
        s_Mode = Mode.PlayingSpeaker;
        s_SpeakerLastPosition = 0;
        for ( int i = 0; i < 5; ++i ){
          AddSpeakerAudioData( s_AudioDataQueue.Dequeue() );
          if ( 0 == s_AudioDataQueue.Count ) break;
        }
        s_SpeakerAudioSource.Play();
        UnityEngine.Debug.Log( s_Mode +" Start" );
      }
    }break;
    
    case Mode.PlayingSpeaker:{
      if ( 0 < s_AudioDataQueue.Count ){
        AddSpeakerAudioData( s_AudioDataQueue.Dequeue() );
        if ( 0 == s_AudioDataQueue.Count ){
          s_SpeakerStopPosition = s_SpeakerLastPosition;
          int position = s_SpeakerAudioSource.timeSamples;
          if ( s_SpeakerStopPosition <= position ){
            s_SpeakerStopPosition += s_SpeakerMaxCount;
          }
        }
      }else if ( s_SpeakerMaxCount <= s_SpeakerStopPosition ){
        int position = s_SpeakerAudioSource.timeSamples;
        if ( position <= ( s_SpeakerStopPosition % s_SpeakerMaxCount ) ){
          s_SpeakerStopPosition -= s_SpeakerMaxCount;
        }
      }else{
        int position = s_SpeakerAudioSource.timeSamples;
        if ( s_SpeakerStopPosition <= position ){
          EditorApplication.isPlaying = false;
          s_Thread = null;
        }
      }
    }break;
    }
  }
  
  static private void AddSpeakerAudioData( float[] data ){
    s_SpeakerAudioClip.SetData( data, s_SpeakerLastPosition );
    s_SpeakerLastPosition = ( s_SpeakerLastPosition + data.Length ) % s_SpeakerMaxCount;
  }
  
  static private int GetMaxCount( AudioClip audio_clip ){
    return audio_clip.samples * audio_clip.channels;
  }
  
  static private int GetCount( int head_position, int tail_position, int max_count ){
    int count = tail_position - head_position;
    if ( count < 0 ) count += max_count;
    return count;
  }
}
