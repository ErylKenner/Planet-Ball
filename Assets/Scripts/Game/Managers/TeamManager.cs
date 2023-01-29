using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    [SerializeField]
    protected Team[] teams;

    public bool ValidTeam(int teamNumber)
    {
        return teamNumber >= 0 && teamNumber < teams.Length;
    }

    public Team GetTeam(int index)
    {
        try
        {
            return teams[index];
        } catch (System.IndexOutOfRangeException)
        {
            Debug.LogError($"${index} is not a valid team index");
            throw;
        }
    }

    public int NumberOfTeams()
    {
        return teams.Length;
    }
}
