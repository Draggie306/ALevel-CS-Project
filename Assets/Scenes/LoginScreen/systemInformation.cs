using UnityEngine;
using TMPro;

// https://docs.unity3d.com/ScriptReference/SystemInfo-graphicsDeviceVersion.html


public class ExampleClass : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Initialising systemInformation.cs");
        string sysInfoGui = GameObject.Find("debugSysData").GetComponent<TextMeshProUGUI>().text;
        string tempCompiledStats = "";

        // Prints "OpenGL 2.0 [2.0 ATI-1.4.40]" on MacBook Pro running Mac OS X 10.4.8
        // Prints "Direct3D 9.0c [atiumdag.dll 7.14.10.471]" on MacBook Pro running Windows Vista
        /*
        Debug.Log($"LowLevelAPIgraphicsDeviceVersion: {SystemInfo.graphicsDeviceVersion}");
        Debug.Log($"systemMemorySize: {SystemInfo.systemMemorySize}");
        Debug.Log($"graphicsMemorySize: {SystemInfo.graphicsMemorySize}");
        Debug.Log($"graphicsDeviceName: {SystemInfo.graphicsDeviceName}");
        Debug.Log($"graphicsDeviceVendor: {SystemInfo.graphicsDeviceVendor}");
        Debug.Log($"graphicsDeviceMultithreaded: {SystemInfo.graphicsMultiThreaded}");

        Debug.Log($"processorCount: {SystemInfo.processorCount}");
        Debug.Log($"processorType: {SystemInfo.processorType}");
        Debug.Log($"operatingSystem: {SystemInfo.operatingSystem}");
        Debug.Log($"operatingSystemFamily: {SystemInfo.operatingSystemFamily}");
        Debug.Log($"deviceUniqueIdentifier: {SystemInfo.deviceUniqueIdentifier}");
        Debug.Log($"deviceName: {SystemInfo.deviceName}");
        Debug.Log($"deviceModel: {SystemInfo.deviceModel}");

        Debug.Log($"supportsRayTracing: {SystemInfo.supportsRayTracing}");
        */

        sysInfoGui = SystemInfo.graphicsDeviceVersion;
        tempCompiledStats = $"LowLevelAPIgraphicsDeviceVersion: {SystemInfo.graphicsDeviceVersion}\nsystemMemorySize: {SystemInfo.systemMemorySize}\ngraphicsMemorySize: {SystemInfo.graphicsMemorySize}\ngraphicsDeviceName: {SystemInfo.graphicsDeviceName}\ngraphicsDeviceVendor: {SystemInfo.graphicsDeviceVendor}\ngraphicsDeviceMultithreaded: {SystemInfo.graphicsMultiThreaded}\nprocessorCount: {SystemInfo.processorCount}\nprocessorType: {SystemInfo.processorType}\noperatingSystem: {SystemInfo.operatingSystem}\noperatingSystemFamily: {SystemInfo.operatingSystemFamily}\ndeviceUniqueIdentifier: {SystemInfo.deviceUniqueIdentifier}\ndeviceName: {SystemInfo.deviceName}\ndeviceModel: {SystemInfo.deviceModel}\nsupportsRayTracing: {SystemInfo.supportsRayTracing}";

        GameObject.Find("debugSysData").GetComponent<TextMeshProUGUI>().text = tempCompiledStats;
        // systemMemorySize
        // graphicsMemorySize
        //Debug.Log(tempCompiledStats);

    }
}