#pragma once

#include <Arduino.h>

namespace Sam3xCanBitTiming {

struct RegisterConfig {
  bool valid = false;
  uint32_t baudrateRegister = 0;
};

struct TimingProfile {
  uint8_t tq;
  uint8_t prop;
  uint8_t phase1;
  uint8_t phase2;
  uint8_t sjw;
};

inline RegisterConfig compute(uint32_t mainClockHz, uint32_t bitrateKbps) {
  static const TimingProfile kProfiles[] = {
      {8, 3, 2, 2, 3},
      {9, 2, 3, 3, 2},
      {10, 3, 3, 3, 3},
      {11, 4, 3, 3, 4},
      {12, 3, 4, 4, 4},
      {13, 4, 4, 4, 4},
      {14, 4, 4, 5, 4},
      {15, 4, 5, 5, 4},
      {16, 5, 5, 5, 4},
      {17, 6, 5, 5, 4},
      {18, 5, 6, 6, 4},
      {19, 6, 6, 6, 4},
      {20, 7, 6, 6, 4},
      {21, 8, 6, 6, 4},
      {22, 7, 7, 7, 4},
      {23, 8, 8, 7, 4},
      {24, 7, 8, 8, 4},
      {25, 8, 8, 8, 4},
  };

  RegisterConfig config{};
  if (bitrateKbps == 0) return config;

  const uint32_t minDivisor = bitrateKbps * 25U * 1000U;
  if (((mainClockHz + (minDivisor - 1U)) / minDivisor) > 128U) return config;
  if (mainClockHz < (bitrateKbps * 8U * 1000U)) return config;

  uint8_t bestTq = 8;
  uint32_t bestRemainder = 0xFFFFFFFFu;

  for (uint8_t tq = 8; tq <= 25; ++tq) {
    const uint32_t divisor = bitrateKbps * (uint32_t)tq * 1000U;
    if ((mainClockHz / divisor) <= 128U) {
      const uint32_t remainder = mainClockHz % divisor;
      if (remainder < bestRemainder) {
        bestRemainder = remainder;
        bestTq = tq;
        if (remainder == 0U) break;
      }
    }
  }

  const uint32_t prescale = mainClockHz / (bitrateKbps * (uint32_t)bestTq * 1000U);
  if (prescale == 0U || prescale > 128U) return config;

  const TimingProfile& profile = kProfiles[bestTq - 8U];
  config.valid = true;
  config.baudrateRegister =
      CAN_BR_PHASE2(profile.phase2 - 1U) |
      CAN_BR_PHASE1(profile.phase1 - 1U) |
      CAN_BR_PROPAG(profile.prop - 1U) |
      CAN_BR_SJW(profile.sjw - 1U) |
      CAN_BR_BRP(prescale - 1U);
  return config;
}

}  // namespace Sam3xCanBitTiming
