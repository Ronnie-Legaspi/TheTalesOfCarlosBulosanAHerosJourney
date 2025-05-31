// File: PartModels.cs
using System;
using System.Collections.Generic;

[Serializable]
public class Part
{
    public int number;
    public string image;
    public string title;
    public string description;
    public bool enabled;
}

[Serializable]
public class PartListWrapper
{
    public List<Part> parts;
}
