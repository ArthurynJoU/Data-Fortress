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
    private java.time.LocalDateTime time;
    private int score;
    private int level;

    protected Score() {}

    public Score(int score, int level) {
        this.score = score;
        this.level = level;
    }

    public void setTime(java.time.LocalDateTime time) {
        this.time = time;
    }

    public Long getId() {
        return id;
    }

    public java.time.LocalDateTime getTime() {
        return time;
    }

    public int getScore() {
        return score;
    }

    public int getLevel() {
        return level;
    }
}