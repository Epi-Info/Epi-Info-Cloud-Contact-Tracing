function SharingRule1(ruleId, isHostOrgUser, userOrgId, responseOrgId)
{
    switch (ruleId)
    {
        case 1:
        default:
            // Organization users can only access the data of there organization.
            return userOrgId == responseOrgId;

        case 2:
            // All users in host organization will have access to all data of all organizations
            // and other Organization users can only access the data of there organization.
            return isHostOrgUser || userOrgId == responseOrgId;

        case 3:
            // All users of all organizations can access all data.
            return true;
    }
}
