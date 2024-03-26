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
    private LinkedList<LinkedListNode<Order>> orders;

    private Tilemap map;
    
    public Mission(Unit unit, Tilemap tilemap)
    {
        orders = new LinkedList<LinkedListNode<Order>>();
        agent = unit;
        map = tilemap;
    }


    //Returns the grid coordinate the agent will be in when the mission is complete
    public Vector3 GetFinalPosition()
    {
        Vector3 finalPosition = orders.Last.Value.Value.destination;
        if(orders.Last.Value.Value is AttackOrder || orders.Last.Value.Value is HoldOrder)
        {
            finalPosition = orders.Last.Previous.Value.Value.origin;
        }

        return finalPosition;
    }


    //This method returns the first order in orders and removes the first node
    public Order ReceiveOrder()
    {
        //This method should not return null
        Order next = null;

        if(orders.Count > 0)
        {
            next = orders.First.Value.Value;
            orders.RemoveFirst();
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
        orders.AddFirst(new LinkedListNode<Order>(order));
    }


    public bool IsComplete()
    {
        return (orders.Count == 0);
    }

    public bool IsAttack()
    {
        return orders.Last.Value.Value is AttackOrder;
    }

    public override string ToString()
    {
        string description = "The mission is: ";

        foreach(LinkedListNode<Order> node in orders)
        {
            description += map.WorldToCell(node.Value.destination) + " " + node.Value.ToString() + " -> ";
        }

        return description;
    }



}
