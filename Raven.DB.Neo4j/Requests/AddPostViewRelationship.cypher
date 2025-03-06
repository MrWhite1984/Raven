MATCH (u:Users{UserId:$userId}), (p:Posts{PostId:$postId})
MERGE (u)-[:View]->(p)