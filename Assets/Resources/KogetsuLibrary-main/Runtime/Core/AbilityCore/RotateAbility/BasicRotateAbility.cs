namespace Kogetsu.Library.Core
{
    public class BasicRotateAbility : MonoBehaviour
    {
        [SerializeField] protected Vector3 Rotate;
        [SerializeField] protected float RoateSpeed = 1;

        private void Update()
        {
            transform.Rotate(RoateSpeed * Time.deltaTime * Rotate);
        }

    }
}
