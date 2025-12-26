using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject[] exits, ends, rooms, enters, doorLocked;

    [SerializeField]
    private GameObject soldier, sniper;

    private List<BoxCollider> placedRooms = new List<BoxCollider>();
    public Transform root;
    public NavMeshSurface navMeshSurface;

    bool IsColliding(BoxCollider previousRoom, BoxCollider newRoomCollider)
    {
        foreach (BoxCollider room in placedRooms)
        {
            if (room != previousRoom && newRoomCollider.bounds.Intersects(room.bounds))
                return true;
        }
        return false;
    }

    public void DestroyLevel()
    {
        Destroy(root.gameObject);
    }

    public IEnumerator GenerateNewLevel(int i)
    {
        yield return new WaitForFixedUpdate();
        placedRooms = new List<BoxCollider>();

        int maxLevel;
        if (i < 4) maxLevel = 3;
        else maxLevel = 4;

        float hp;
        if(i < 5) hp = 50f;
        else if(i < 10) hp = 70f;
        else hp = 120f;

        float angularSpeed;
        if(i < 2) angularSpeed = 520f;
        else if(i < 5) angularSpeed = 640f;
        else if (i < 7) angularSpeed = 720f;
        else angularSpeed = 850f;

        float speed;
        if (i < 2) speed = 6f;
        else if(i < 5) speed = 7f;
        else if(i < 7) speed = 8.5f;
        else speed = 9f;

        float alertRadius;
        if (i < 2) alertRadius = 25f;
        else if(i < 5) alertRadius = 30f;
        else if(i < 7) alertRadius = 35f;
        else alertRadius = 40f;

        float damage;
        if (i < 2) damage = 4f;
        else if(i < 5) damage = 6f;
        else if(i < 7) damage = 8f;
        else damage = 10f;

        float runSpeed;
        if (i < 2) runSpeed = 13f;
        else if(i < 5) runSpeed = 16f;
        else if(i < 7) runSpeed = 19f;
        else runSpeed = 23f;
        root = GameObject.Instantiate(enters[UnityEngine.Random.Range(0, enters.Length)]).transform;
        yield return GenerateRoom(root, 1, maxLevel);

        navMeshSurface.BuildNavMesh();
        Transform player = FindObjectOfType<Player>().transform;
        player.position = root.position;
        player.rotation = root.rotation;
        player.GetComponent<CharacterController>().enabled = true;
        GetComponent<GameManager>().changingLevel = false;
        player.GetComponent<Player>().currentWeapon.ammoCount += 100;
        player.GetComponent<FPSController>().runningSpeed = runSpeed;
        GetComponent<UI>().SetAmmo(player.GetComponent<Player>().currentWeapon.currentClips, player.GetComponent<Player>().currentWeapon.ammoCount);

        EnemySpawner[] enemySpawners = root.gameObject.GetComponentsInChildren<EnemySpawner>();
        for(int j = 0; j < enemySpawners.Length; j++)
        {
            Transform enemy = GameObject.Instantiate(enemySpawners[j].isSniper ? sniper : soldier).transform;
            enemy.GetComponent<Enemy>().hp = hp;
            enemy.GetComponent<Enemy>().weapon.damage = damage;
            Enemy.alertRadius = alertRadius;
            enemy.GetComponent<NavMeshAgent>().angularSpeed = angularSpeed;
            enemy.GetComponent<NavMeshAgent>().speed = speed;
            enemy.position = enemySpawners[j].transform.position;
            enemy.rotation = enemySpawners[j].transform.rotation;
            enemy.GetComponent<NavMeshAgent>().Warp(enemy.position);
        }
    }

    IEnumerator GenerateRoom(Transform room, int currentLevel, int maxLevel)
    {
        BoxCollider roomCollider = room.GetComponent<BoxCollider>();

        if (IsColliding(room.parent != null ? room.parent.parent.GetComponent<BoxCollider>() : null, roomCollider))
        {
            Transform door = GameObject.Instantiate(doorLocked[UnityEngine.Random.Range(0, doorLocked.Length)]).transform;
            door.SetParent(room.parent);
            door.localPosition = Vector3.zero;
            door.localRotation = Quaternion.identity;
            Destroy(room.gameObject);
            yield break;
        }

        placedRooms.Add(roomCollider);
        bool passageIsExists = false;
        for (int i = 0; i < room.GetComponent<Block>().passages.Length; i++)
        {
            Transform nextRoom;
            if (currentLevel < maxLevel)
                nextRoom = GameObject.Instantiate(
                    !passageIsExists && i == room.GetComponent<Block>().passages.Length - 1 ?
                    rooms[UnityEngine.Random.Range(0, rooms.Length)] :
                    (UnityEngine.Random.Range(0, 6) != 0 ?
                    rooms[UnityEngine.Random.Range(0, rooms.Length)] : ends[UnityEngine.Random.Range(0, ends.Length)]
                    )).transform;
            else
                nextRoom = GameObject.Instantiate(
                    !passageIsExists && i == room.GetComponent<Block>().passages.Length - 1 ?
                    exits[UnityEngine.Random.Range(0, exits.Length)] :
                    (UnityEngine.Random.Range(0, 2) == 1 ?
                    exits[UnityEngine.Random.Range(0, exits.Length)] : ends[UnityEngine.Random.Range(0, ends.Length)]
                )).transform;

            nextRoom.SetParent(room.GetComponent<Block>().passages[i]);
            nextRoom.localPosition = Vector3.zero;
            nextRoom.localRotation = Quaternion.identity;
            yield return new WaitForFixedUpdate();

            yield return GenerateRoom(nextRoom, currentLevel + 1, maxLevel);
        }
    }
}
