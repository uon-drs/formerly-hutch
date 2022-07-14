import {
  Heading,
  HStack,
  Text,
  LinkBox,
  LinkOverlay,
  VStack,
  Box,
  Button,
  Flex,
} from "@chakra-ui/react";
import { Link } from "react-router-dom";

export const ResultsModifier = ({
  title,
  children,
  onDelete,
  onUpdate,
  ...p
}) => (
  <VStack
    bg="gray.100"
    borderColor="gray.300"
    borderWidth={2}
    borderRadius={5}
    h="100%"
    p={4}
    align="stretch"
    _hover={{
      borderColor: "blue.500",
      color: "blue.500",
      bg: "gray.50",
    }}
    {...p}
  >
    <HStack>
      <Flex w="full">
        <Heading as="h3" size="md">
          {title}
        </Heading>
        <Button colorScheme="red" ml={"auto"} onClick={onDelete}>
          Delete
        </Button>
      </Flex>
    </HStack>
    {children}
    <Button ml={"auto"} onClick={onUpdate}>
      Update
    </Button>
  </VStack>
);