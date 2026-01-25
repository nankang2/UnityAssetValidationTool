using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValidationResults : MonoBehaviour
{
    public string assetPath;         
    public string message;  
    public ValidationSeverity severity;

    public bool selected;
    public Action autoFixAction;
}
