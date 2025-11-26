# XpressNet Specification - Chapter 5: Messages Overview

**Publisher:** Lenz Elektronik GmbH

---

## 5.1 Command overview of information and responses sent to XpressNet devices (including a PC) from the command station

The following table summarizes the various responses sent to XpressNet devices. The precise meaning of the respective data bytes is described in the appropriate section within section 3 above. In the following tables, N in the header byte indicates the number of data bytes that follow.

| V3 | Instruction                                               | Call Byte    | Header      | Data Byte 1 | Data Byte 2 | Date Byte 3 | Data Byte 4 | Data Byte 5 | Data Byte 6 | Data Byte 7 |
|----|-----------------------------------------------------------|--------------|-------------|-------------|-------------|-------------|-------------|-------------|-------------|-------------|
| x  | Normal inquiry                                            | P+0x40+GA    |             |             |             |             |             |             |             |             |
| x  | Request Acknowledgement from Device                       | P+0x00+GA    |             |             |             |             |             |             |             |             |
| x  | TBD (Future Command)                                      | P+0x20+GA    |             |             |             |             |             |             |             |             |
| x  | Normal operation resumed                                  | 0x60         | 0x61        | 0x01        | 0x60        |             |             |             |             |             |
| x  | Track power Off                                           | 0x60         | 0x61        | 0x00        | 0x61        |             |             |             |             |             |
| x  | Emergency Stop                                            | 0x60         | 0x81        | 0x00        | 0x81        |             |             |             |             |             |
| x  | Service Mode entry                                        | 0x60         | 0x61        | 0x02        | 0x63        |             |             |             |             |             |
| x  | Feedback Broadcast                                        | 0xA0         | 0x40 + N    | ADR_1       | DATA_1      | ADR_2       | DATA_2      | etc.        | etc.        | X-Or        |
| x  | Programming info. "short-circuit"                         | P+0x60+GA    | 0x61        | 0x12        | X-Or        |             |             |             |             |             |
| x  | Programming info. "Data byte not found"                   | P+0x60+GA    | 0x61        | 0x13        | X-Or        |             |             |             |             |             |
| x  | Programming info. "Command station busy"                  | P+0x60+GA    | 0x61        | 0x1F        | X-Or        |             |             |             |             |             |
| x  | Programming info. "Command station ready"                 | P+0x60+GA    | 0x61        | 0x11        | X-Or        |             |             |             |             |             |
| x  | Service Mode response for Register and Paged Mode         | P+0x60+GA    | 0x63        | 0x10        | EE          | DATA        | X-Or        |             |             |             |
| x  | Service Mode response for Direct CV mode                  | P+0x60+GA    | 0x63        | 0x14        | CV          | DATA        | X-Or        |             |             |             |
|    | Software-version (X-Bus V1 and V2)                        | P+0x60+GA    | 0x62        | 0x21        | DATA        | X-Or        |             |             |             |             |
| x  | Software-version (XpressNet only)                         | P+0x60+GA    | 0x63        | 0x21        | DATA_1      | DATA_2      | X-Or        |             |             |             |
| x  | Command station status indication response                | P+0x60+GA    | 0x62        | 0x22        | DATA        | X-Or        |             |             |             |             |
| x  | Transfer Errors                                           | P+0x60+GA    | 0x61        | 0x80        | X-Or        |             |             |             |             |             |
| x  | Command station busy response                             | P+0x60+GA    | 0x61        | 0x81        | X-Or        |             |             |             |             |             |
| x  | Instruction not supported by command station              | P+0x60+GA    | 0x61        | 0x82        | X-Or        |             |             |             |             |             |
| x  | Accessory Decoder information response                    | P+0x60+GA    | 0x42        | Addr        | DATA        | X-Or        |             |             |             |             |
|    | Locomotive is available for operation                     | P+0x60+GA    | 0x83        | Loco addr   | Loco data 1 | Loco data 2 | X-Or        |             |             |             |
|    | Locomotive is being operated by another device            | P+0x60+GA    | 0xA3        | Loco addr   | Loco data 1 | Loco data 2 | X-Or        |             |             |             |
|    | Locomotive is available for operation                     | P+0x60+GA    | 0x84        | Loco addr   | Loco data 1 | Loco data 2 | ModSel      | X-Or        |             |             |
|    | Locomotive is being operated by another device            | P+0x60+GA    | 0xA4        | Loco addr   | Loco data 1 | Loco data 2 | ModSel      | X-Or        |             |             |
| x  | Locomotive information normal locomotive                  | P+0x60+GA    | 0xE4        | Identifica tion | Speed   | FKTA        | FKTB        | X-Or        |             |             |
| x  | Locomotive information for a locomotive in a multi-unit   | P+0x60+GA    | 0xE5        | Identifica tion | Speed   | FKTA        | FKTB        | MTR         | X-Or        |             |
| x  | Locomotive information for the Multi-unit address         | P+0x60+GA    | 0xE2        | Identifica tion | Speed   | X-Or        |             |             |             |             |
| x  | Locomotive information for a locomotive in a Double Header | P+0x60+GA   | 0xE6        | Identifica tion | Speed   | FKTA        | FKTB        | Addr High   | Addr Low    | X-Or        |
| x  | Locomotive is being operated by another device response (XpressNet only) | P+0x60+GA | 0xE3 | 0x40 | Addr High | Addr Low | X-Or |  |  |  |
| x  | Function status response (XpressNet only)                 | P+0x60+GA    | 0xE3        | 0x50        | STAT 0      | STAT 1      | X-Or        |             |             |             |
| x  | Locomotive information response for address retrieval requests (XpressNet only) | P+0x60+GA | 0xE3 | 0x30 + K | Addr High | Addr Low | X-Or |  |  |  |
|    | Double Header available                                   | P+0x60+GA    | 0xC5        | 0x04        | Loco addr 1 | Loco data 1 | Loco data 2 | Loco addr 2 | X-Or        |             |
|    | Double Header occupied                                    | P+0x60+GA    | 0xC5        | 0x05        | Loco addr 1 | Loco data 1 | Loco data 2 | Loco addr 2 | X-Or        |             |
|    | Double Header available                                   | P+0x60+GA    | 0xC6        | 0x04        | Loco addr 1 | Loco data 1 | Loco data 2 | Loco addr 2 | ModSel      | X-Or        |
|    | Double Header occupied                                    | P+0x60+GA    | 0xC6        | 0x05        | Loco addr 1 | Loco data 1 | Loco data 2 | Loco addr 2 | ModSel      | X-Or        |
| x  | Double Header error response (X-Bus V1 and V2)            | P+0x60+GA    | 0x61        | 0x80 + F    | X-Or        |             |             |             |             |             |
| x  | XpressNet MU+DH error message response                    | P+0x60+GA    | 0xE1        | 0x80 + F    | X-Or        |             |             |             |             |             |

## 5.2 Command overview of requests transmitted sent from XpressNet devices (including a PC) to the command station

The following table summarizes the various requests sent by the PC to the command station through the LI100. The precise meaning of the respective data bytes is described in the appropriate section within section 4 above. In the following tables, N in the header byte indicated the number of data bytes that follow.

| V3 | Instruction                                              | Header   | Identification     | Data Byte 1 | Data Byte 2 | Data Byte 3 | Data Byte 4 | Data Byte 6 | Data Byte 6 |
|----|----------------------------------------------------------|----------|--------------------|-------------|-------------|-------------|-------------|-------------|-------------|
|    | Acknowledgement Response                                 | 0x20     | 0x20               |             |             |             |             |             |             |
| x  | Resume operations request                                | 0x21     | 0x81               | 0xA0        |             |             |             |             |             |
| x  | Stop operations request (emergency off)                  | 0x21     | 0x80               | 0xA1        |             |             |             |             |             |
| x  | Stop all locomotives request (emergency stop)            | 0x80     | 0x80               |             |             |             |             |             |             |
|    | Emergency stop a locomotive (X-Bus V1 and V2)            | 0x91     | Loco addr          | X-Or        |             |             |             |             |             |
| x  | Emergency stop a locomotive (XpressNet)                  | 0x92     | Addr High          | Addr Low    | X-Or        |             |             |             |             |
|    | Emergency stop selected locomotives (X-Bus V1 and V2)    | 0x90 + N | Loco addr 1        | Loco addr 2 | etc.        | Loco addr N | X-Or        |             |             |
| x  | Register Mode read request (Register Mode)               | 0x22     | 0x11               | REG         | X-Or        |             |             |             |             |
| x  | Direct Mode CV read request (CV mode)                    | 0x22     | 0x15               | CV          | X-Or        |             |             |             |             |
| x  | Paged Mode read request (Paged Mode)                     | 0x22     | 0x14               | CV          | X-Or        |             |             |             |             |
| x  | Request for Service Mode results                         | 0x21     | 0x10               | 0x31        |             |             |             |             |             |
| x  | Register Mode write request (Register Mode)              | 0x23     | 0x12               | REG         | DATA        | X-Or        |             |             |             |
| x  | Direct Mode CV write request (CV mode)                   | 0x23     | 0x16               | CV          | DATA        | X-Or        |             |             |             |
| x  | Paged Mode write request (Paged mode)                    | 0x23     | 0x17               | CV          | DATA        | X-Or        |             |             |             |
| x  | Command station software-version request                 | 0x21     | 0x21               | 0x00        |             |             |             |             |             |
| x  | Command station status request                           | 0x21     | 0x24               | 0x05        |             |             |             |             |             |
|    | Set command station power-up mode                        | 0x22     | 0x22               | 00000M00    | X-Or        |             |             |             |             |
| x  | Accessory Decoder information request                    | 0x42     | Addr               | Nibble      | X-Or        |             |             |             |             |
| x  | Accessory Decoder operation request                      | 0x52     | Addr               | DAT         | X-Or        |             |             |             |             |
|    | Locomotive information requests (X-Bus V1)               | 0xA1     | Loco addr          | X-Or        |             |             |             |             |             |
|    | Locomotive information requests (X-Bus V1 and V2)        | 0xA2     | Loco addr          | ModSel      | X-Or        |             |             |             |             |
| x  | Locomotive information requests (XpressNet only)         | 0xE3     | 0x00               | Addr High   | Addr Low    | X-Or        |             |             |             |
| x  | Function status request (XpressNet only)                 | 0xE3     | 0x07               | Addr High   | Addr Low    | X-Or        |             |             |             |
|    | Locomotive operations (X-Bus V1)                         | 0xB3     | Loco addr          | Loco data 1 | Loco data 2 | X-Or        |             |             |             |
|    | Locomotive operations (X-Bus V2)                         | 0xB4     | Loco addr          | Loco data 1 | Loco data 2 | ModSel      | X-Or        |             |             |
| x  | Locomotive speed and direction operations (XpressNet only) | 0xE4   | 0x10 0x11 0x12 0x13 | Addr High | Addr Low  | Speed       | X-Or        |             |             |
| x  | Function operation instructions (XpressNet only)         | 0xE4     | 0x20 0x21 0x22     | Addr High   | Addr Low    | Group       | X-Or        |             |             |
| x  | Set function state (XpressNet only)                      | 0xE4     | 0x24 0x25 0x26     | Addr High   | Addr Low    | Group       | X-Or        |             |             |
|    | Establish Double Header                                  | 0xC3     | 0x05               | Loco addr 1 | Loco addr 2 | X-Or        |             |             |             |
|    | Dissolve Double Header                                   | 0xC3     | 0x04               | Loco addr 1 | Loco addr 2 | X-Or        |             |             |             |
| x  | Establish Double Header                                  | 0xE5     | 0x43               | ADR1 H      | ADR1 L      | ADR2 H      | ADR2 L      | X-Or        |             |
| x  | Dissolve Double Header                                   | 0xE5     | 0x43               | ADR1 H      | ADR1 L      | 0x00        | 0x00        | X-Or        |             |
| x  | Operations Mode Programming byte mode write request      | 0xE6     | 0x30               | Addr High   | Addr Low    | 0xEC + C    | CV          | DATA        | X-Or        |
| x  | Operations Mode programming bit mode write request       | 0xE6     | 0x30               | Addr High   | Addr Low    | 0xE8 + C    | CV          | DATA        | X-Or        |
| x  | Add a locomotive to a multi-unit request                 | 0xE4     | 0x40 + R           | Addr High   | Addr Low    | MTR         | X-Or        |             |             |
| x  | Remove a locomotive from a Multi-unit request            | 0xE4     | 0x42               | Addr High   | Addr Low    | MTR         | X-Or        |             |             |
| x  | Address inquiry member of a Multi-unit request           | 0xE4     | 0x01 + R           | MTR         | Addr High   | Addr Low    | X-Or        |             |             |
| x  | Address inquiry Multi-unit request                       | 0xE2     | 0x03 + R           | MTR         | X-Or        |             |             |             |             |
| x  | Address inquiry locomotive at command station stack request | 0xE3  | 0x05 + R           | Addr High   | Addr Low    | X-Or        |             |             |             |
| x  | Delete locomotive from command station stack request     | 0xE3     | 0x44               | Addr High   | Addr Low    | X-Or        |             |             |             |
