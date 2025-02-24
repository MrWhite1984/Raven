MATCH (u:Users{UserId:$userId}), (p:Posts{PostId:$postId})
CREATE (u)-[:Like]->(p)