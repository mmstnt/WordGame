using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogCharacter : MonoBehaviour
{
    private Coroutine currentMoveCoroutine;
    private Vector2 endPosition;
    public Image image;

    public void initialize(string chImageID,Vector2 site, bool dire) 
    {
        image = GetComponent<Image>();
        image.sprite = DataManager.instance.characterImageDataList.getData(chImageID);
        image.color = Color.gray;
        image.SetNativeSize();

        transform.position = site;
        transform.rotation = Quaternion.Euler(0, (dire ? 0 : 180), 0);
    }

    public void moveTo(string chImageID, Vector2 vector, bool dire, float speed)
    {
        if (currentMoveCoroutine != null)
        {
            StopCoroutine(currentMoveCoroutine);
            transform.position = endPosition;
        }

        image.sprite = DataManager.instance.characterImageDataList.getData(chImageID);
        image.SetNativeSize();
        transform.rotation = Quaternion.Euler(0, (dire ? 0 : 180), 0);
        endPosition = vector;

        currentMoveCoroutine = StartCoroutine(moveCoroutine(vector, speed));
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

    public IEnumerator characterFade(float speed, bool isfade) 
    {
        Color color = image.color;
        color.a = isfade ? 1 : 0;

        while (isfade ? color.a >= 0f : color.a <= 1.0f) 
        {
            // 逐漸減少透明度
            color.a += (isfade ? -speed : speed) * Time.deltaTime;
            image.color = color;
            yield return null; // 等待下一影幀
        }
    }
}
