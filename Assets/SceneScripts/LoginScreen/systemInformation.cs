using UnityEngine;
using TMPro;

// https://docs.unity3d.com/ScriptReference/SystemInfo-graphicsDeviceVersion.html
/// <summary>
/// Gets the system information and displays it in tmproGUI for debugging
/// </summary>

public class SystemDataDevelopmentDisplayer : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"Initialising systemInformation.cs on GO {gameObject.name}");

        string tempCompiledStats;

        tempCompiledStats = $"DEVELOPMENT INFORMATION\n\nLowLevelAPIgraphicsDeviceVersion: {SystemInfo.graphicsDeviceVersion}\nsystemMemorySize: {SystemInfo.systemMemorySize}\ngraphicsMemorySize: {SystemInfo.graphicsMemorySize}\ngraphicsDeviceName: {SystemInfo.graphicsDeviceName}\ngraphicsDeviceVendor: {SystemInfo.graphicsDeviceVendor}\ngraphicsDeviceMultithreaded: {SystemInfo.graphicsMultiThreaded}\nprocessorCount: {SystemInfo.processorCount}\nprocessorType: {SystemInfo.processorType}\noperatingSystem: {SystemInfo.operatingSystem}\noperatingSystemFamily: {SystemInfo.operatingSystemFamily}\ndeviceUniqueIdentifier: {SystemInfo.deviceUniqueIdentifier}\ndeviceName: {SystemInfo.deviceName}\ndeviceModel: {SystemInfo.deviceModel}\nsupportsRayTracing: {SystemInfo.supportsRayTracing}";

        GameObject.Find("debugSysData").GetComponent<TextMeshProUGUI>().text = tempCompiledStats;
        // systemMemorySize
        // graphicsMemorySize
        //Debug.Log(tempCompiledStats);
    }
}