function parseScoreData(scoreData) {
  if (typeof scoreData !== "string") {
    return null;
  }

  var values = {};
  var parts = scoreData.split(";");
  for (var i = 0; i < parts.length; i++) {
    var separator = parts[i].indexOf(":");
    if (separator <= 0) {
      return null;
    }

    var key = parts[i].substring(0, separator).trim();
    var rawValue = parts[i].substring(separator + 1).trim();
    if (!key || !/^-?\d+$/.test(rawValue) ||
        Object.prototype.hasOwnProperty.call(values, key)) {
      return null;
    }

    values[key] = parseInt(rawValue, 10);
  }

  return values;
}

function getDailyRewardUsage() {
  var today = new Date().toISOString().split("T")[0];
  var dataKey = "DailyReward_" + today;

  try {
    var userData = server.GetUserReadOnlyData({
      PlayFabId: currentPlayerId,
      Keys: [dataKey]
    });

    var usedCount = 0;
    if (userData.Data && userData.Data[dataKey]) {
      var storedCount = userData.Data[dataKey].Value;
      if (!/^\d+$/.test(storedCount)) {
        log.error("Contador diario de recompensas inválido", storedCount);
        return { success: false, message: "No se pudo validar el límite diario." };
      }
      usedCount = parseInt(storedCount, 10);
    }

    return { success: true, dataKey: dataKey, usedCount: usedCount };
  } catch (e) {
    log.error("No se pudo consultar el límite diario", e);
    return { success: false, message: "No se pudo consultar el límite diario." };
  }
}

handlers.GetDailyRewardStatus = function () {
  var usage = getDailyRewardUsage();
  if (!usage.success) {
    return { success: false, message: usage.message };
  }

  return {
    success: true,
    remainingToday: Math.max(0, 3 - usage.usedCount)
  };
};

handlers.GrantMinigameReward = function (args) {
  if (!args || typeof args.gameId !== "string" ||
      typeof args.scoreData !== "string") {
    return { success: false, message: "Datos de juego inválidos." };
  }

  var scores = parseScoreData(args.scoreData);
  if (!scores) {
    return { success: false, message: "scoreData inválido." };
  }

  var rewardAmount = 0;
  if (args.gameId === "tictactoe") {
    if (scores.wins === undefined || scores.total === undefined ||
        scores.total < 1 || scores.total > 6 ||
        scores.wins < 0 || scores.wins > scores.total) {
      return { success: false, message: "Puntuación de TicTacToe inválida." };
    }

    rewardAmount = scores.wins * 5;
  } else if (args.gameId === "memory") {
    if (scores.pairs === undefined || scores.attempts === undefined ||
        scores.timeLeft === undefined ||
        (scores.pairs !== 6 && scores.pairs !== 8 && scores.pairs !== 12) ||
        scores.attempts < scores.pairs || scores.attempts > 1000 ||
        scores.timeLeft < 0 || scores.timeLeft > 120) {
      return { success: false, message: "Puntuación de Memory inválida." };
    }

    var penalty = Math.max(0, (scores.attempts - scores.pairs) * 2);
    var timeBonus = Math.floor(scores.timeLeft / 10);
    rewardAmount = Math.max(0, 20 - penalty + timeBonus);
  } else {
    return { success: false, message: "Minijuego no reconocido." };
  }

  if (rewardAmount <= 0) {
    return {
      success: true,
      reward: 0,
      newBalance: null,
      message: "Sin recompensa."
    };
  }

  var usage = getDailyRewardUsage();
  if (!usage.success) {
    return { success: false, message: usage.message };
  }
  var dataKey = usage.dataKey;
  var usedCount = usage.usedCount;

  if (usedCount >= 3) {
    return {
      success: false,
      message: "Límite diario de recompensas alcanzado.",
      newBalance: null,
      remainingToday: 0
    };
  }

  var newUsedCount = usedCount + 1;
  var updateData = {};
  updateData[dataKey] = newUsedCount.toString();
  try {
    server.UpdateUserReadOnlyData({
      PlayFabId: currentPlayerId,
      Data: updateData
    });
  } catch (e) {
    log.error("No se pudo reservar el uso diario de recompensa", e);
    return { success: false, message: "No se pudo registrar el límite diario." };
  }

  var addResult;
  try {
    addResult = server.AddUserVirtualCurrency({
      PlayFabId: currentPlayerId,
      VirtualCurrency: "SD",
      Amount: rewardAmount
    });
  } catch (e) {
    log.error("AddUserVirtualCurrency falló", e);
    var apiError = e && e.apiErrorInfo && e.apiErrorInfo.apiError;
    var errorDetails = apiError
      ? apiError.error + " (" + apiError.errorCode + "): " + apiError.errorMessage
      : e && (e.errorMessage || e.message);
    if (!errorDetails) {
      errorDetails = JSON.stringify(e) || "error sin detalles";
    }

    var remainingToday = 3 - newUsedCount;
    try {
      var rollbackData = {};
      rollbackData[dataKey] = usedCount.toString();
      server.UpdateUserReadOnlyData({
        PlayFabId: currentPlayerId,
        Data: rollbackData
      });
      remainingToday = 3 - usedCount;
    } catch (rollbackError) {
      log.error("No se pudo devolver el uso diario reservado", rollbackError);
    }

    return {
      success: false,
      message: "Error al acreditar la recompensa: " + errorDetails,
      remainingToday: remainingToday
    };
  }

  return {
    success: true,
    reward: rewardAmount,
    newBalance: addResult.Balance,
    remainingToday: 3 - newUsedCount
  };
};
