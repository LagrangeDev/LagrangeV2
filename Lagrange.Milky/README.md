<div align="center">

# Lagrange.Milky

_[Milky](https://github.com/SaltifyDev/milky) protocol implementation based on [Lagrange.Core V2](https://github.com/LagrangeDev/LagrangeV2)_

</div>

## Document

https://lagrangedev.github.io/Lagrange.Milky.Document

## Feature List

Current Milky version: [77c6cbd](https://github.com/SaltifyDev/milky/tree/77c6cbd6fb131d581fbb4c128a540eadfbcbd475)

### communication

- [ ] SSE (wontimpl)
- [x] WebSocket
- [x] WebHook

### api

#### system

- [x] /get_login_info
- [x] /get_impl_info
- [x] /get_user_profile
- [x] /get_friend_list
- [x] /get_friend_info
- [x] /get_group_list
- [x] /get_group_info
- [x] /get_group_member_list
- [x] /get_group_member_info
- [x] /get_cookies
- [ ] /get_csrf_token

#### message

- [x] /send_private_message
- [x] /send_group_message
- [x] /get_message
  - [x] friend
  - [x] group
  - [ ] temp (wontimpl)
- [ ] /get_history_messages
  - [x] friend
  - [x] group
  - [ ] temp (wontimpl)
  - [ ] start_message_seq
- [x] /get_resource_temp_url
- [ ] /get_forwarded_messages
- [x] /recall_private_message
- [x] /recall_group_message
- [ ] /mark_message_as_read

#### friend

- [x] /send_friend_nudge
- [ ] /send_profile_like
- [ ] /get_friend_requests
- [ ] /accept_friend_request
- [ ] /reject_friend_request

#### group

- [x] /set_group_name
- [ ] /set_group_avatar
- [x] /set_group_member_card
- [x] /set_group_member_special_title
- [ ] /set_group_member_admin
- [ ] /set_group_member_mute
- [ ] /set_group_whole_mute
- [ ] /kick_group_member
- [ ] /get_group_announcement_list
- [ ] /send_group_announcement
- [ ] /delete_group_announcement
- [ ] /get_group_essence_messages
- [ ] /set_group_essence_message
- [x] /quit_group
- [x] /send_group_message_reaction
- [x] /send_group_nudge
- [x] /get_group_notifications
- [ ] /accept_group_request
- [ ] /reject_group_request
- [ ] /accept_group_invitation
- [ ] /reject_group_invitation

#### file

- [x] /upload_private_file - No return value
- [x] /upload_group_file
- [ ] /get_private_file_download_url
- [x] /get_group_file_download_url
- [ ] /get_group_files
- [ ] /move_group_file
- [ ] /rename_group_file
- [x] /delete_group_file
- [ ] /create_group_folder
- [ ] /rename_group_folder
- [ ] /delete_group_folder

### event

- [x] bot_offline
- [x] message_receive
- [ ] message_recall
  - [x] friend
  - [x] group
  - [ ] temp (wontimpl)
- [x] friend_request
- [ ] group_join_request
- [ ] group_invited_join_request
- [ ] group_invitation
- [ ] friend_nudge
- [ ] friend_file_upload
- [ ] group_admin_change
- [ ] group_essence_message_change
- [ ] group_member_increase
- [x] group_member_decrease
- [ ] group_name_change
- [ ] group_message_reaction
- [ ] group_mute
- [ ] group_whole_mute
- [x] group_nudge
- [ ] group_file_upload

### segment

#### incoming

- [x] text
- [x] mention
- [x] mention_all
- [ ] face
- [x] reply
- [x] image
- [x] record
- [x] video
- [ ] file
- [x] forward
- [ ] market_face
- [ ] light_app
- [ ] xml

#### outgoing

- [x] text
- [x] mention
- [x] mention_all
- [ ] face
- [x] reply
- [x] image
- [x] record
- [ ] video
- [x] forward

#### forward

- [x] text
- [ ] mention - (wontimpl) forward not supported
- [ ] mention_all - (wontimpl) forward not supported
- [ ] face
- [ ] reply
- [ ] image
- [ ] record
- [ ] video
- [ ] forward
