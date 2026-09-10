"""Split eligible match statistics by computer and online play.

Revision ID: 0003_split_match_stats
Revises: 0002_discord_login
Create Date: 2026-08-03
"""

from collections.abc import Sequence

from alembic import op
import sqlalchemy as sa


revision: str = "0003_split_match_stats"
down_revision: str | None = "0002_discord_login"
branch_labels: str | Sequence[str] | None = None
depends_on: str | Sequence[str] | None = None


def upgrade() -> None:
    with op.batch_alter_table("user_stats") as batch_op:
        batch_op.add_column(
            sa.Column(
                "computer_victories",
                sa.BigInteger(),
                server_default=sa.text("0"),
                nullable=False,
            )
        )
        batch_op.add_column(
            sa.Column(
                "computer_matches_played",
                sa.BigInteger(),
                server_default=sa.text("0"),
                nullable=False,
            )
        )
        batch_op.add_column(
            sa.Column(
                "online_victories",
                sa.BigInteger(),
                server_default=sa.text("0"),
                nullable=False,
            )
        )
        batch_op.add_column(
            sa.Column(
                "online_matches_played",
                sa.BigInteger(),
                server_default=sa.text("0"),
                nullable=False,
            )
        )
        batch_op.create_check_constraint(
            "computer_victories_non_negative",
            "computer_victories >= 0",
        )
        batch_op.create_check_constraint(
            "computer_matches_played_non_negative",
            "computer_matches_played >= 0",
        )
        batch_op.create_check_constraint(
            "online_victories_non_negative",
            "online_victories >= 0",
        )
        batch_op.create_check_constraint(
            "online_matches_played_non_negative",
            "online_matches_played >= 0",
        )


def downgrade() -> None:
    with op.batch_alter_table("user_stats") as batch_op:
        batch_op.drop_constraint(
            "online_matches_played_non_negative",
            type_="check",
        )
        batch_op.drop_constraint(
            "online_victories_non_negative",
            type_="check",
        )
        batch_op.drop_constraint(
            "computer_matches_played_non_negative",
            type_="check",
        )
        batch_op.drop_constraint(
            "computer_victories_non_negative",
            type_="check",
        )
        batch_op.drop_column("online_matches_played")
        batch_op.drop_column("online_victories")
        batch_op.drop_column("computer_matches_played")
        batch_op.drop_column("computer_victories")
