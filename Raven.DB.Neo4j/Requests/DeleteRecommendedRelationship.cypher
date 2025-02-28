MATCH (u:Users{UserId:$userId})-[r:Recomended]->(p:Posts{PostId:$postId})
DELETE r