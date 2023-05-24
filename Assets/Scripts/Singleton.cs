/*
 * Singleton.cs
 * 
 * - Unity Implementation of Singleton template
 * 
 */

using UnityEngine;

/// <summary>
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// 
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>
public class Singleton : MonoBehaviour
{
	public static Singleton Instance { get; private set; }
}
