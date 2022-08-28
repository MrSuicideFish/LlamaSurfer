
public class GameplayScreenController : GameScreenControllerBase
{
    private int points_internal = 0;

    public void UpdatePoints()
    {
        int newPoints = GameManager.Instance.points;
        ((GameplayScreenView)_view).SetPointsTo(points_internal, newPoints);
        points_internal = newPoints;
    }
}