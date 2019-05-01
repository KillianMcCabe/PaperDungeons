﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : Mob
{
    const float moveSpeed = 6f;

    NavNode currentNodePosition;
    List<NavNode> pathing;
    List<NavNode> _nodesWithinMovementRange;
    Coroutine pathingCoroutine;

    public bool HasKey = false;
    public static Player Instance;
    public bool acceptingInput = true;

    int speed = 6;
    int strength = 5;
    int armor = 2;
    int health = 10;

    public NavNode NodePosition
    {
        get { return currentNodePosition; }
    }

    public Vector2 WorldPosition
    {
        get { return transform.position; }
        // get { return currentNodePosition.WorldPosition; }
    }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // get current position on node grid
        currentNodePosition = NavigationGrid.Instance.GetNode(new Vector2(transform.position.x, transform.position.y));
        currentNodePosition.Mob = this;

        Debug.Log("Player is at " + currentNodePosition);

        acceptingInput = true;

        NavNode.OnNodeClicked += HandleNodeClicked;
    }

    // TODO: GameManager should call this and all references to GameManager.Instance.playersTurn should be removed
    public void StartTurn()
    {
        // acceptingInput = true;
        _nodesWithinMovementRange = NavigationGrid.Instance.GetNodesWithinRange(currentNodePosition, speed);
    }

    public override void ReceiveAttack(int attackPower)
    {
        int damage = Mathf.Max(attackPower - armor, 0);
        health -= damage;
        MessageLogController.Instance.AddMessage($"You received {damage} damage!", MessageLogController.MessageType.Warning);

        if (health <= 0)
        {
            MessageLogController.Instance.AddMessage("You died.", MessageLogController.MessageType.Warning);
            GameManager.Instance.GameOver();
        }
    }

    // Update is called once per frame
    private IEnumerator Pathing()
    {
        while (pathing.Count > 0)
        {
            // pop
            NavNode nextNode = pathing[0];
            pathing.RemoveAt(0);

            // check if the path is blocked by an object e.g. door
            if (nextNode.InteractableObject != null && nextNode.Blocked)
            {
                nextNode.InteractableObject.Interact();

                // notify GameManager that players turn has ended
                GameManager.Instance.playersTurn = false;

                yield break;
            }
            // check if the path is blocked by an enemy
            else if (nextNode.Mob != null)
            {
                nextNode.Mob.ReceiveAttack(strength);

                // notify GameManager that players turn has ended
                GameManager.Instance.playersTurn = false;

                yield break;
            }
            else
            {
                // move to new position
                while (transform.position != (Vector3)nextNode.WorldPosition)
                {
                    transform.position = Vector2.MoveTowards(transform.position, nextNode.WorldPosition, Time.deltaTime * moveSpeed);
                    yield return null;
                }

                currentNodePosition.Mob = null;
                currentNodePosition = nextNode;
                currentNodePosition.Mob = this;
                transform.position = currentNodePosition.WorldPosition;
            }

            NavigationGrid.Instance.CalculateLighting(); // TODO: move into GameManager.cs
        }

        // check if we stopped on an item
        if (currentNodePosition.InteractableObject != null)
        {
            currentNodePosition.InteractableObject.Interact();
        }

        // notify GameManager that players turn has ended
        GameManager.Instance.playersTurn = false;
    }

    private void HandleNodeClicked(NavNode clickedNavNode)
    {
        if (!GameManager.Instance.playersTurn || !acceptingInput)
        {
            return;
        }

        pathing = NavigationGrid.Instance.CalculatePath(currentNodePosition, clickedNavNode);
        if (pathing != null)
        {
            if (pathingCoroutine != null)
            {
                StopCoroutine(pathingCoroutine);
            }
            pathingCoroutine = StartCoroutine(Pathing());
        }
    }

    void OnDrawGizmos()
    {
        if (pathing != null)
        {
            for (int i = 0; i < pathing.Count; i++)
            {
                if (i == 0)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(currentNodePosition.WorldPosition, pathing[i].WorldPosition);
                }
                else
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(pathing[i-1].WorldPosition, pathing[i].WorldPosition);
                }
            }
        }

        if (_nodesWithinMovementRange != null)
        {
            for (int i = 0; i < _nodesWithinMovementRange.Count; i++)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(_nodesWithinMovementRange[i].WorldPosition, Vector3.one);
            }
        }
    }
}
