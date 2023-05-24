using UnityEngine;
using System.Collections;

public class InputTweak : Input
{
  static Vector3 lastMousePosition;
  public static bool passThrough = false;

  public static new int touchCount{
    get{
      if (passThrough || Input.touchSupported){
        return Input.touchCount;
      }
      else{
        return (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0)) ? 1 : 0;
      }
    }
  }

  public static new Touch[] touches{
    get{
      if (passThrough || Input.touchSupported){
        return Input.touches;
      }
      else{
        if( Input.GetMouseButton(0) || Input.GetMouseButtonUp(0) || Input.GetMouseButtonDown(0) ){
          return new Touch[]{ GetTouch(0) };
        }
        else{
          return new Touch[]{};
        }
      }
    }
  }

  public static new bool touchSupported {
    get{
      return Input.touchSupported || (!passThrough && Input.mousePresent);
    }
  }

  public static new Touch GetTouch(int index){

    if (passThrough || Input.touchSupported){
      return Input.GetTouch(index);
    }
    else{

      if (Input.GetMouseButtonUp(0)){
        lastMousePosition = Input.mousePosition;
        Touch t = new Touch();
        t.position = Input.mousePosition;
        t.phase = TouchPhase.Ended;
        return t;
      }
      else if(Input.GetMouseButtonDown(0)){
        lastMousePosition = Input.mousePosition;

        Touch t = new Touch();
        t.position = Input.mousePosition;
        t.phase = TouchPhase.Began;
        return t;
      }
      else if (Input.GetMouseButton(0)){
        Touch t = new Touch();
        t.position = Input.mousePosition;
        t.phase = (lastMousePosition == Input.mousePosition) ? TouchPhase.Stationary : TouchPhase.Moved;

        lastMousePosition = Input.mousePosition;
        return t;
      }
      else{
        throw new System.ArgumentOutOfRangeException($"Can only spoof one touch. touch requested: {index}");
      }

    }
  }
}

