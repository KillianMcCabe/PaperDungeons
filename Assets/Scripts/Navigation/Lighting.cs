﻿using System.Collections.Generic;
using UnityEngine;

public class Lighting
{
    private const float LightRange = 8;

    LightNode[,] _lightNodeGrid;
    public LightNode[,] LightNodeGrid { get { return _lightNodeGrid; } }

    private int _lightGridSizeX = 0;
    private int _lightGridSizeY = 0;

    LayerMask _navNodeWallLayerMask = LayerMask.GetMask("NavNode_Wall");

    public void InitialiseLightGrid(int gridMinWorldX, int gridMinWorldY)
    {
        _lightGridSizeX = NavigationGrid.Instance.navGridSizeX - 1;
        _lightGridSizeY = NavigationGrid.Instance.navGridSizeY - 1;
        _lightNodeGrid = new LightNode[_lightGridSizeX, _lightGridSizeY];

        // create and initialise all blocks in the light grid
        for (int x = 0; x < _lightGridSizeX; x++)
        {
            for (int y = 0; y < _lightGridSizeY; y++)
            {
                LightNode newLightNode = new LightNode();
                newLightNode.Init(
                    worldPos: new Vector2(gridMinWorldX + x + 0.5f, gridMinWorldY + y + 0.5f)
                );
                newLightNode.navNodes = new List<NavNode>();
                for (int i = 0; i <= 1; i++)
                {
                    for (int j = 0; j <= 1; j++)
                    {
                        if (NavigationGrid.Instance.NodeGrid[x + i, y + j] != null)
                            newLightNode.navNodes.Add(NavigationGrid.Instance.NodeGrid[x + i, y + j]);
                    }
                }
                _lightNodeGrid[x, y] = newLightNode;
            }
        }
    }

    /// <summary>
    /// Recalculates lighting
    /// </summary>
    public void Recalculate()
    {
        // start by setting all nodes to dark
        foreach (NavNode navNode in NavigationGrid.Instance.NodeGrid)
        {
            if (navNode != null)
            {
                navNode.Visible = false;
            }
        }

        // check all light nodes
        foreach (LightNode lightNode in _lightNodeGrid)
        {
            Vector2 towardsPlayer = Player.Instance.WorldPosition - lightNode.WorldPosition;
            float dist = towardsPlayer.magnitude;
            // check if within light range
            if (dist < LightRange)
            {
                // check if within light of sight from player
                RaycastHit2D hit = Physics2D.Raycast(lightNode.WorldPosition, (towardsPlayer).normalized, towardsPlayer.magnitude, _navNodeWallLayerMask);
                if (hit.collider == null)
                {
                    // Debug.DrawLine(lightNode.WorldPosition, lightNode.WorldPosition + towardsPlayer, Color.green, 2f);
                    lightNode.Visible = true;

                    // mark all nodes connected to this light node as visible
                    foreach (NavNode n in lightNode.navNodes)
                        n.Visible = true;
                }
                else
                {
                    // Debug.DrawLine(lightNode.WorldPosition, hit.point, Color.red, 2f);
                    lightNode.Visible = false;
                }
            }
        }
    }
}