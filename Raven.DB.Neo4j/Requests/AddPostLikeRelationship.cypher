MATCH (u:Users{UserId:$userId}), (p:Posts{PostId:$postId})
MERGE (u)-[:Like]->(p)