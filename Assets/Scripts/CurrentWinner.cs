using Firebase.Firestore;

[FirestoreData]

public struct CurrentWinner
{
    [FirestoreProperty]

    public string winner {get; set;}
}
