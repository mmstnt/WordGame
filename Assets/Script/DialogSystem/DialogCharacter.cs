using System.Collections;
using UnityEngine;

public class DialogCharacter : MonoBehaviour
{
    private Coroutine currentMoveCoroutine;
    private Vector2 endPosition;

    public void moveTo(Vector2 vector, float time)
    {
        if (currentMoveCoroutine != null)
        {
            StopCoroutine(currentMoveCoroutine);
            transform.position = endPosition;
        }

        endPosition = vector;

        currentMoveCoroutine = StartCoroutine(moveCoroutine(vector, time));
    }

    private IEnumerator moveCoroutine(Vector2 vector, float speed)
    {
        float distance = Vector2.Distance(transform.position, vector);
        float time = distance / speed;
        float duration = 0;

        Vector2 startPos = transform.position;

        while (duration < time)
        {
            transform.position = Vector2.Lerp(startPos, vector, duration / time);
            duration += Time.deltaTime;
            yield return null;
        }

        transform.position = vector;
        currentMoveCoroutine = null;
    }
}
