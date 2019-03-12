using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class IDedObject<T> : MonoBehaviour where T : MonoBehaviour
{
    /// <summary>
    /// Private storage for ID.
    /// </summary>
    [SerializeField]
    [Tooltip("Unique ID for this object. <RMB for tools>")]
    [ContextMenuItem("Validate This ID", "ValidateIDWithRefresh")]
    [ContextMenuItem("Assign Next Available ID", "ReassignID")]
    private byte p_ID;

    /// <summary>
    /// Readonly byte used to identify this object.
    /// </summary>
    public byte ID { get => p_ID; }

    /// <summary>
    /// Reassigns the object's ID to the next available ID.
    /// </summary>
    /// <returns>Returns the previous ID.</returns>
    protected void ReassignID()
    {
        if (allIDs.Count == 0) 
        {
            p_ID = 0;
        }
        else
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                if (!allIDs.Contains(i))
                {
                    p_ID = i;
                    return;
                }
            }
            Debug.LogError("[IDReassigned] There are no more available IDs to be assigned, code might perform in weird ways past this point.", this);
        }
    }

    public void ValidateID()
    {
        List<byte> allIDsButMine = allIDs;
        allIDsButMine.Remove(p_ID);
        if (allIDsButMine.Contains(p_ID))
        {
            byte prevID = p_ID;
            ReassignID();
            Debug.Log("[IDReassigned] An object already exists with the ID of " + prevID + ", this object's ID has been automatically reassigned to " + p_ID, this);
        }
        else
        {
            Debug.Log("[IDValidated] This object has a unique ID!");
        }
    }

    public void ValidateIDWithRefresh()
    {
        RefreshAllObjects();
        ValidateID();
    }

    protected virtual void Awake() {
        IDedObject<T>.AddObject(this);
        ValidateID();
    }

    protected virtual void OnDestroy()
    {
        IDedObject<T>.RemoveObject(this);
    }

    #region GLOBAL REFERENCING

    private static List<IDedObject<T>> allObjects = new List<IDedObject<T>>();
    private static List<byte> allIDs = new List<byte>();

    /// <summary>
    /// Adds the new object to a list and checks the legality of it's ID.
    /// </summary>
    /// <param name="newObject">The object to add.</param>
    public static void AddObject(IDedObject<T> newObject)
    {
        IDedObject<T>.allObjects.Add(newObject);
        allIDs.Add(newObject.ID);
    }

    /// <summary>
    /// Remove an object from the object and ID lists.
    /// </summary>
    /// <param name="deletingObject">The object to remove.</param>
    public static void RemoveObject(IDedObject<T> deletingObject)
    {
        allObjects.Remove(deletingObject);
        allIDs.Remove(deletingObject.ID);
    }

    /// <summary>
    /// Refreshes all allObjects list.
    /// </summary>
    public static void RefreshAllObjects() {
        allObjects = ((IDedObject<T>[])FindObjectsOfType(typeof(IDedObject<T>))).ToList();
        allIDs = allObjects.Select(obj => obj.ID).ToList();
    }

    /// <summary>
    /// Searches for an object with an equal ID to givenID.
    /// </summary>
    /// <param name="givenID">ID to search for.</param>
    /// <returns>Returns the found object with ID equal to givenID. Returns a default value of T if no object of givenID exists.</returns>
    public static IDedObject<T> GetObject(byte givenID)
    {
        if (foundObject?.ID == givenID) return foundObject;
        // RefreshAllObjects();
        foundObject = allObjects.Find(obj => obj.ID == givenID);
        if (foundObject == default(IDedObject<T>))
            Debug.Log("[ObjectIDSearch|Type:" + typeof(T) + "] No object of the type '" + typeof(T) + "' exists with the ID of " + givenID.ToString() + ". (Returning default value for " + typeof(T) + ")");
        return foundObject;
    }
    private static IDedObject<T> foundObject;
    #endregion
}
