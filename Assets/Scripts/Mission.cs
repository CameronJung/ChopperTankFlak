using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/*
 * The mission class represents a series of directives that specify how an AI or a player tells a unit to move
 */
public class Mission
{
    private Unit agent;
    private LinkedList<Order> Orders;

    private Tilemap map;
    
    public Mission(Unit unit, Tilemap tilemap)
    {
        Orders = new LinkedList<Order>();
        agent = unit;
        map = tilemap;

        
        
    }


    //Returns the grid coordinate the agent will be in when the mission is complete
    public Vector3 GetFinalPosition()
    {
        Vector3 finalPosition = Orders.Last.Value.destination;
        if(Orders.Last.Value is AttackOrder || Orders.Last.Value is HoldOrder)
        {
            finalPosition = Orders.Last.Previous.Value.origin;
        }

        return finalPosition;
    }


    //This method returns the first order in orders and removes the first node
    public Order ReceiveOrder()
    {
        //This method should not return null
        Order next = null;

        if(Orders.Count > 0)
        {
            next = Orders.First.Value;
            Orders.RemoveFirst();
        }
        else
        {
            //If this method is called on a completed (empty) mission, than we return a hold order
            HoldOrder holdOrder = new HoldOrder(agent.transform.position, agent.transform.position, agent);
            next = holdOrder;
            Debug.LogWarning("A completed Mission was queried for more orders, this shouldn't happen and a hold order was sent to avoid a crash");
        }

        return next;
    }


    public void AddOrder(Order order)
    {
        bool isValid = true;
        if(Orders.Count >= 1)
        {
            if (Orders.Last.Value is AttackOrder)
            {
                //No order within the same mission should ever have a destination to a tile it attacks
                //This is an admittedly lazy means of solving a very frustrating bug
                //if no solution is found make a bug report for it
                isValid = (Orders.Last.Value.destination != order.destination);
            }
        }
        

        if (isValid)
        {
            Orders.AddFirst(new LinkedListNode<Order>(order));
        }
        else
        {
            Debug.Log("An erroneous order was ignored while constructing a mission");
        }
        
    }


    public bool IsComplete()
    {
        return (Orders.Count == 0);
    }

    public bool IsAttack()
    {
        return Orders.Last.Value is AttackOrder;
    }

    public override string ToString()
    {
        string description = "The mission is: ";

        foreach(Order node in Orders)
        {
            description += map.WorldToCell(node.destination) + " " + node.ToString() + " -> ";
        }

        return description;
    }



}
