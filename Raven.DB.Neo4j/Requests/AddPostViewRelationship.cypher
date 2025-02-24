MATCH (u:Users{UserId:$userId}), (p:Posts{PostId:$postId})
CREATE (u)-[:View]->(p)