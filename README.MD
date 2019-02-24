(Proof-of-Concept,WIP)

# CS-GO-Observer-Unity-HUD  
This is the test program to show HUD(and others) by using GameStateIntegration,mirv_pgl,afx_interop.  

### How does it work?
Unity scripts receives main data by GameStateIntegration,and draws into CS:GO screen as 3D data via afx_interop.  


# How-to-use  
(not perfect,will describe later)
1. Install GSI cfg  
2. Open HLAE  
3. Launch CS:GO by HLAE  
4. Launch Unity by HLAE CustomLoader with AfxHookUnity  
5. connect HLAE and CS:GO (afx_interop 1).  
6. Start Unity preview

## Credits
C# GSI Library from : https://github.com/rakijah/CSGSI  
afx_interop : https://github.com/advancedfx/afx-unity-interop  
HLAE-Server for C# : https://github.com/xNWP/HLAE-Server/tree/csharp  

affected by https://github.com/osztenkurden/CS-GO-Observer-Custom-HUD
