package game.datafortress.backend.model;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;

@Entity
public class Score {
    @Id
    @GeneratedValue(strategy=GenerationType.IDENTITY)
    private Long id;
    private int time;
    private int score;
    private int level;

    protected Score() {}

    public Score(int time, int score, int level) {
        this.time = time;
        this.score = score;
        this.level = level;
    }

    public Long getId() {
        return id;
    }

    public int getTime() {
        return time;
    }

    public int getScore() {
        return score;
    }

    public int getLevel() {
        return level;
    }
}