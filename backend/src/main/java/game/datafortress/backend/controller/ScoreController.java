package game.datafortress.backend.controller;

import game.datafortress.backend.model.Score;
import game.datafortress.backend.repository.ScoreRepository;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;

@RestController
public class ScoreController {
    private ScoreRepository scoreRepository;

    public ScoreController(ScoreRepository scoreRepository) {
        this.scoreRepository = scoreRepository;
    }

    @PostMapping("/scores")
    public Score postScoreRepository(@RequestBody Score score) {
        scoreRepository.save(score);
        return score;
    }

    @GetMapping("/scores/top")
    public List<Score> getScoreRepository() {
        return scoreRepository.findTop3ByOrderByScoreDesc();
    }
}

