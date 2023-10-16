using ChatService.DTOs;
using ChatService.Models;
using Google.Cloud.Firestore;

namespace ChatService
{
    public class FireStoreService
    {
        private readonly FirestoreDb db;
        public FireStoreService() {
            string path = "doancnpmnhom4-6bc5e-firebase-adminsdk-lt7t0-d931a2f994.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            db = FirestoreDb.Create("doancnpmnhom4-6bc5e");
            
        }

        public async Task<ChatResponseDTO> GetTrip(string tripId)
        {
            ChatResponseDTO chatResponseDTO = new ChatResponseDTO();
            chatResponseDTO.TripId = Guid.Parse(tripId);
            List<ChatMessage> chatMessages = new List<ChatMessage>();
            DocumentReference docRef = db.Collection("Chat").Document(tripId);
            
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            
            if (snapshot.Exists)
            {
                Console.WriteLine("Document data for {0} document:", snapshot.Id);
                Dictionary<string, object> city = snapshot.ToDictionary();
                foreach (KeyValuePair<string, object> pair in city)
                {
                    if (pair.Key == "driverId")
                    {
                        chatResponseDTO.DriverId = pair.Value == null ? Guid.Empty : Guid.Parse(pair.Value.ToString());
                    }
                    if (pair.Key == "passengerId")
                    {
                        chatResponseDTO.PassengerId = pair.Value == null ? Guid.Empty : Guid.Parse(pair.Value.ToString());
                    }
                    if (pair.Key == "createTime")
                    {
                        chatResponseDTO.TripCreatedTime = pair.Value == null ? DateTime.MaxValue : DateTime.Parse(pair.Value.ToString());
                    }
                }

                Query messageRef = docRef.Collection("messages");
                QuerySnapshot allMessageSnapShot = await messageRef.GetSnapshotAsync();
                foreach (DocumentSnapshot documentSnapshot in allMessageSnapShot.Documents)
                {
                    ChatMessage chatMessage = new ChatMessage();
                    Dictionary<string, object> messages = documentSnapshot.ToDictionary();
                    foreach (KeyValuePair<string, object> pair in messages)
                    {
                        if(pair.Key == "senderName")
                        {
                            chatMessage.SenderName = pair.Value == null ? string.Empty : pair.Value.ToString();
                        } 
                        if(pair.Key == "senderId")
                        {
                            chatMessage.SenderId = pair.Value == null ? Guid.Empty : Guid.Parse(pair.Value.ToString());
                        }
                        if (pair.Key == "message")
                        {
                            chatMessage.Message = pair.Value == null ? string.Empty : pair.Value.ToString();
                        }
                        if (pair.Key == "date")
                        {
                            chatMessage.SendTime = pair.Value == null ? DateTime.MaxValue : DateTime.Parse(pair.Value.ToString());
                        }
                        Console.WriteLine("{0}: {1}", pair.Key, pair.Value);
                    }
                    chatMessages.Add(chatMessage);
                    Console.WriteLine("");
                }
                chatResponseDTO.Messages = chatMessages;
                return chatResponseDTO;
            }
            else
            {
                Console.WriteLine("Document does not exist!");
                return new ChatResponseDTO();
            }
        }
    }
}
