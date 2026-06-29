package game.datafortress.backend.repository;

import game.datafortress.backend.model.Score;
import org.springframework.data.repository.CrudRepository;

import java.util.List;

public interface ScoreRepository extends CrudRepository<Score, Long> {
    List<Score> findTop3ByOrderByScoreDesc();
}
