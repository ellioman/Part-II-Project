using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// This struct encodes a string and a boolean in a pair.
/// 
/// Unfortunately, a Generics version cannot be serialized, so have to make do with this :(.
/// </summary>
[Serializable]
public struct StringBoolPair
{
    // Have to wrap private variables, as opoosed to automatic version, as only this works with serialization.
    [SerializeField]
    private string _first;
    public string first { get { return _first; } private set { _first = value; } }

    [SerializeField]
    private bool _second;
    public bool second { get { return _second; } private set { _second = value; } }


    public StringBoolPair(string first, bool second)
    {
        _first = first;
        _second = second;
    }

    public static StringBoolPair Create(string first, bool second)
    {
        return new StringBoolPair(first, second);
    }
}